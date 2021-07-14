using Akka.Actor;
using Newtonsoft.Json;
using PrerenderDotNet.Messages;
using PrerenderDotNet.WebDriver;
using System;
using System.Diagnostics;
using System.Drawing;
using System.Threading;

namespace PrerenderDotNet.Actors
{
    public class PrerenderActor : ReceiveActor
    {
        private readonly IActorRef activityObserverRef;
        private HttpAwareWebDriver driver;

        public PrerenderActor(IActorRef activityObserverRef)
        {
            this.activityObserverRef = activityObserverRef;

            ReceiveAsync<RenderingRequestMessage>(async message => {
                var id = Guid.NewGuid();                
                if (driver == null)
                {
                    NewDriver();
                }

                try
                {
                    var windowSize = message.IsMobile ? new Size(480, 320) : new Size(1920, 1200);
                    driver.Manage().Window.Size = windowSize;

                    driver.Capabilities["window-size"] = $"{windowSize.Width},{ windowSize.Height}";

                    activityObserverRef.Tell(new RenderingStartMessage(
                        id,
                        driver?.ProcessId ?? -1,
                        Thread.CurrentThread.ManagedThreadId,
                        message.StartDate,
                        driver.Capabilities));

                    var timer = Stopwatch.StartNew();
                    var response = await driver.GetResponseAsync(message.Url, CancellationToken.None);
                    string html = null;
                    byte[] screenShot = null;
                    if (response.Status == 200)
                    {
                        driver.WaitForPrerenderReady(10);

                        if (message.Activity == Messages.Activity.RenderHtml)
                        {
                            html = driver.PageSource;
                        }
                        else
                        {
                            screenShot = driver.GetScreenshot();
                        }
                    }
                    
                    activityObserverRef.Tell(new RenderingEndMessage(id, response.Status, timer.Elapsed));
                    Sender.Tell(new RenderingResponseMessage(
                        Self.Path.ToStringWithAddress(), html, screenShot, response.Status, response.RedirectUrl));
                }
                catch (Exception ex)
                {
                    Sender.Tell(new RenderingResponseMessage(
                        Self.Path.ToStringWithAddress(), JsonConvert.SerializeObject(ex), null, 500, null));
                }
                finally
                {
                    driver.Url = "about:blank";
                }
            });
        }

        public override void AroundPreRestart(Exception cause, object message)
        {
            NewDriver();
            base.AroundPreRestart(cause, message);
        }

        public override void AroundPreStart()
        {
            NewDriver();
            base.AroundPreStart();
        }

        public override void AroundPostStop()
        {
            driver?.Dispose();
            driver = null;
            base.AroundPostStop();
        }

        private void NewDriver()
        {
            if (driver != null)
            {
                driver.Dispose();
                driver = null;
            }

            if (driver == null)
            {
                driver = new HttpAwareWebDriver();
                activityObserverRef.Tell(new BrowserCreatedMessage(
                    driver.ProcessId,
                    Thread.CurrentThread.ManagedThreadId,
                    driver.Capabilities));
            }
        }

        public static Props Props(IActorRef activityObserverRef)
        {
            return Akka.Actor.Props.Create(() => new PrerenderActor(activityObserverRef));
        }
    }
}

using Akka.Actor;
using PrerenderDotNet.Clients;
using PrerenderDotNet.Messages;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Dynamic;
using System.Linq;

namespace PrerenderDotNet.Actors
{
    public class BrowserObserverActor: ReceiveActor
    {
        private readonly IProcessClient processClient;
        private readonly Dictionary<string, dynamic> browsers;

        public BrowserObserverActor(IProcessClient processClient)
        {
            browsers = new Dictionary<string, dynamic>();

            Receive<BrowserCreatedMessage>(message => {
                processClient.AddId(message.ProcessId);
                UpdateBrowsers(Sender.Path.ToStringWithAddress(), message);
            });

            Receive<RenderingStartMessage>(message => {
                UpdateBrowsers(Sender.Path.ToStringWithAddress(), message);
            });

            Receive<RenderingEndMessage>(message => {
                UpdateBrowsers(Sender.Path.ToStringWithAddress(), message);
            });

            Receive<BrowserInfoRequest>(message => {
                Sender.Tell(GetBrowserInfo());
            });
        }

        private BrowserInfoResponse GetBrowserInfo()
        {
            var infos = from pair in browsers
                        let processId = (int)pair.Value.processId
                        let theadId = (int)pair.Value.theadId
                        let started = (int)pair.Value.started
                        let completed = (int)pair.Value.completed
                        where processId > 0
                        let process = Process.GetProcessById(processId)
                        let capabilities = pair.Value.capabilities
                        let lastActivity = (DateTime?)pair.Value.lastActivity
                        select new BrowserInfo(
                            pair.Key,
                            theadId,
                            started,
                            completed,
                            new ProcessInfo(
                            processId,                            
                            process.ProcessName,
                            process.StartTime,
                            new CpuTime(
                              process.TotalProcessorTime,
                              process.UserProcessorTime,
                              process.PrivilegedProcessorTime),
                            new MemoryUsage(
                              process.WorkingSet64,
                              process.PeakWorkingSet64,
                              process.Threads.Count)),
                            capabilities,
                            lastActivity);

            return new BrowserInfoResponse(infos);
        }       

        private void UpdateBrowsers(string actorName, BrowserCreatedMessage message)
        {
            if (!browsers.TryGetValue(actorName, out var browser))
            {
                browser = new ExpandoObject();
                browsers.Add(actorName, browser);
            }

            browser.processId = message.ProcessId;
            browser.started = 0;
            browser.completed = 0;
            browser.theadId = message.ThreadId;
            browser.capabilities = message.Capabilities;
            browser.lastActivity = DateTime.Now;
        }

        private void UpdateBrowsers(string actorName, RenderingStartMessage message) 
        {
            if (!browsers.TryGetValue(actorName, out var browser))
            {
                browser = new ExpandoObject();
                browsers.Add(actorName, browser);
            }

            browser.started = ((int)browser.started) + 1;
            browser.processId = message.ProcessId;
            browser.theadId = message.ThreadId;
            browser.capabilities = message.Capabilities;
            browser.lastActivity = (DateTime?)message.StartDate;
        }

        private void UpdateBrowsers(string actorName, RenderingEndMessage message)
        {
            if (!browsers.TryGetValue(actorName, out var browser))
            {
                return;
            }

            var lastActivity = (DateTime?)browser.lastActivity;
            browser.completed = ((int)browser.completed) + 1;
            browser.lastActivity = (DateTime?)(lastActivity.HasValue ? lastActivity.Value.Add(message.Duration) : null);
        }

        public static Props Props(IProcessClient processClient)
        {
            return Akka.Actor.Props.Create(() => new BrowserObserverActor(processClient));
        }
    }
}

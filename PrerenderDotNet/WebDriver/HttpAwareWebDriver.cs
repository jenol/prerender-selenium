using Newtonsoft.Json;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Sockets;
using System.Net.WebSockets;
using System.Threading;
using System.Threading.Tasks;

namespace PrerenderDotNet.WebDriver
{
    public class HttpAwareWebDriver : IWebDriver, IJavaScriptExecutor, IDisposable
    {
        private readonly ChromeDriver driver;
        private readonly ChromeDriverService service;
        private readonly string webSocketDebuggerUrl;

        public HttpAwareWebDriver()
        {
            var port = GetAvailablePort();
            service = ChromeDriverService.CreateDefaultService();
            service.HideCommandPromptWindow = true;
            var chromeOptions = new ChromeOptions();

            chromeOptions.AddArguments("--headless",
                                       "--disable-gpu",
                                       "--ignore-certificate-errors",
                                       "--disable-extensions",
                                       "--no-sandbox",
                                       "--disable-dev-shm-usage",
                                       $"--remote-debugging-port={port}");

            driver = new ChromeDriver(service, chromeOptions);
            ProcessId = service.ProcessId;
            webSocketDebuggerUrl = GetWebSocketDebuggerUrl(port);
            Capabilities = new Dictionary<string, object>();

            void AddCapability(string name)
            {
                if (driver.Capabilities.HasCapability(name))
                {
                    Capabilities.Add(name, driver.Capabilities.GetCapability(name));
                }
            }

            AddCapability("browserVersion");
            AddCapability("browserName");
            AddCapability("platform");
        }

        public byte[] GetScreenshot() => ((ITakesScreenshot) driver).GetScreenshot().AsByteArray;

        public async Task<WebDriverHttpResponse> GetResponseAsync(string url, CancellationToken cancellationToken)
        {            
            using var webSocketDebugger = new ClientWebSocket
            {
                Options =
                {
                    KeepAliveInterval = TimeSpan.Zero
                }
            };
            try
            {
                await webSocketDebugger.ConnectAsync(new Uri(webSocketDebuggerUrl), cancellationToken);
                await webSocketDebugger.SendAsJson(new { id = 1, method = "Network.enable" }, cancellationToken);

                Url = url;

                var redirects = new BlockingCollection<string>();
                while (!cancellationToken.IsCancellationRequested && webSocketDebugger.State == WebSocketState.Open)
                {
                    var message = await webSocketDebugger.ReceiveJsonAsync<dynamic>(cancellationToken);
                    var method = (string)message?.method;
                    var messageParams = message?["params"];
                    
                    if (method != "Network.responseReceived")
                    {
                        var location = (string)messageParams?.headers?.Location;
                        if (location != null)
                        {
                            redirects.Add(location, cancellationToken);
                        }

                        continue;
                    }

                    if (redirects.Any())
                    {
                        return new WebDriverHttpResponse(Url, 301, redirects.ToArray()[redirects.Count - 1]);
                    }

                    var response = messageParams?.response;

                    var responseUrl = ((string)response?.url)?.ToLower();

                    if (responseUrl == Url.ToLower())
                    {
                        var status = (int)response?.status;
                        return new WebDriverHttpResponse(this.Url, status);
                    }
                }
            }
            catch (WebSocketException e)
            {
                // TODO: log it, reconnect or let it bubble up (the actor will be restarted)
                throw e;
            }
            catch (Exception e)
            {
                // TODO: log it, reconnect or let it bubble up (the actor will be restarted)
                throw e;
            }

            return null;
        }

        private static string GetWebSocketDebuggerUrl(int port)
        {
            using var client = new HttpClient();
            var webRequest = new HttpRequestMessage(HttpMethod.Get, $"http://localhost:{port}/json");
            var response = client.Send(webRequest);
            using var reader = new StreamReader(response.Content.ReadAsStream());
            var text = reader.ReadToEnd();
            dynamic[] data = JsonConvert.DeserializeObject<dynamic[]>(text);
            return (string)data.FirstOrDefault().webSocketDebuggerUrl;
        }

        public string Url { get => ((IWebDriver)driver).Url; set => ((IWebDriver)driver).Url = value; }
        public string Title => ((IWebDriver)driver).Title;
        public string PageSource => ((IWebDriver)driver).PageSource;
        public string CurrentWindowHandle => ((IWebDriver)driver).CurrentWindowHandle;
        public ReadOnlyCollection<string> WindowHandles => ((IWebDriver)driver).WindowHandles;
        public int ProcessId { get; }
        public Dictionary<string, object> Capabilities { get; }
        public void Close() => ((IWebDriver)driver).Close();
        public void Quit() => ((IWebDriver)driver).Quit();
        public IOptions Manage() => ((IWebDriver)driver).Manage();
        public INavigation Navigate() => ((IWebDriver)driver).Navigate();
        public ITargetLocator SwitchTo() => ((IWebDriver)driver).SwitchTo();
        public IWebElement FindElement(By by) => ((ISearchContext)driver).FindElement(by);
        public ReadOnlyCollection<IWebElement> FindElements(By by) => ((ISearchContext)driver).FindElements(by);

        public void Dispose()
        {
            try
            {
                driver.Close();
                driver.Quit();
                service.Dispose();
            }
            catch (WebDriverException)
            {
                // TODO: log this: happens when the chromedriver process is gone already 
            }
        }

        private static int GetAvailablePort()
        {
            TcpListener l = new TcpListener(IPAddress.Loopback, 0);
            l.Start();
            int port = ((IPEndPoint)l.LocalEndpoint).Port;
            l.Stop();
            return port;
        }

        object IJavaScriptExecutor.ExecuteScript(string script, params object[] args) => (driver as IJavaScriptExecutor).ExecuteScript(script, args);
        object IJavaScriptExecutor.ExecuteAsyncScript(string script, params object[] args) => (driver as IJavaScriptExecutor).ExecuteAsyncScript(script, args);
    }
}

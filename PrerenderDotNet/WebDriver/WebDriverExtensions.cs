using OpenQA.Selenium;
using System.Threading;

namespace PrerenderDotNet
{
    public static class WebDriverExtensions
    {
        public static void WaitForPrerenderReady(this IJavaScriptExecutor driver, int timeoutInSeconds)
        {    
            driver.ExecuteScript(@"
setTimeout(() => { window.prerenderReady = true }, 5000);
if (window.prerenderReady === undefined) {
    setInterval(() => { window.prerenderReady = document.getElementsByTagName('vn-responsive-footer') !== undefined; }, 100);
}
");

            do
            {               
                if (driver.IsPrerenderReady())
                {
                    break;
                }

                Thread.Sleep(1000);
                timeoutInSeconds--;
            } while (timeoutInSeconds > 0);
        }

        private static bool IsPrerenderReady(this IJavaScriptExecutor driver)
        {
            return (bool)driver.ExecuteScript("return window.prerenderReady === true");
        }
    }
}

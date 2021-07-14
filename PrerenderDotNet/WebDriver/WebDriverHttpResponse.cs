namespace PrerenderDotNet.WebDriver
{
    public class WebDriverHttpResponse
    {
        public WebDriverHttpResponse (string url, int status, string redurectUrl = null)
        {
            Url = url;
            Status = status;
            RedirectUrl = redurectUrl;
        }

        public string Url { get; }
        public int Status { get; }
        public string RedirectUrl { get; }
    }
}

namespace PrerenderDotNet.Messages
{
    public class RenderingResponseMessage
    {
        public RenderingResponseMessage(string actorPath, string html, byte[] screenShot, int httpStatus, string redirectUrl)
        {
            ActorPath = actorPath;
            Html = html;
            ScreenShot = screenShot;
            HttpStatus = httpStatus;
            RedirectUrl = redirectUrl;
        }

        public string ActorPath { get; }
        public string Html { get; }
        public int HttpStatus { get; }
        public string RedirectUrl { get; }
        public byte[] ScreenShot { get; internal set; }
    }
}

using System;

namespace PrerenderDotNet.Messages 
{
    public class RenderingRequestMessage
    {
        public RenderingRequestMessage(string url, bool isMobile, Activity activity, DateTime startDate)
        {
            Url = url;
            IsMobile = isMobile;
            Activity = activity;
            StartDate = startDate;
        }

        public bool IsMobile { get; }
        public string Url { get; }
        public DateTime StartDate { get; }
        public Activity Activity { get; }
    }

    public enum Activity
    {
        RenderHtml,
        Screenshot,
    }
}
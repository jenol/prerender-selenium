using System;

namespace PrerenderDotNet.Messages
{
    public class RenderingEndMessage
    {
        public RenderingEndMessage(Guid id, int httpStatusCode, TimeSpan duration)
        {
            Id = id;
            Duration = duration;
            HttpStatusCode = httpStatusCode;
        }

        public Guid Id { get; }
        public TimeSpan Duration { get; }
        public int HttpStatusCode { get; }
    }
}

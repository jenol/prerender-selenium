using System;
using System.Collections.Generic;

namespace PrerenderDotNet.Messages
{
    public class RenderingStartMessage
    {
        public RenderingStartMessage(Guid id, int processId, int threadId, DateTime startDate, IReadOnlyDictionary<string, object> capabilities)
        {
            Id = id;
            ProcessId = processId;
            ThreadId = threadId;
            StartDate = startDate;
            Capabilities = capabilities;
        }

        public Guid Id { get; }
        public int ProcessId { get; }
        public int ThreadId { get; }
        public DateTime StartDate { get; }

        public IReadOnlyDictionary<string, object> Capabilities { get; }
    }
}

using System;
using System.Collections.Generic;

namespace PrerenderDotNet.Messages
{
    public class BrowserCreatedMessage
    {
        public BrowserCreatedMessage(int processId, int threadId, IReadOnlyDictionary<string, object> capabilities)
        {            
            ProcessId = processId;
            ThreadId = threadId;
            Capabilities = capabilities;
        }

        public Guid Id { get; }
        public int ProcessId { get; }
        public int ThreadId { get; }
        public IReadOnlyDictionary<string, object> Capabilities { get; }
    }
}

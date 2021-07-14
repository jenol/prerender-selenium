using System;
using System.Collections.Generic;

namespace PrerenderDotNet.Messages
{
    public class BrowserInfoResponse
    {
        public BrowserInfoResponse(IEnumerable<BrowserInfo> browsers)
        {
            Browsers = browsers;
        }

        public IEnumerable<BrowserInfo> Browsers { get; }
    }

    public class BrowserInfo
    {
        public BrowserInfo(
            string actorName,
            int theadId,
            int started,
            int completed,
            ProcessInfo process,
            IReadOnlyDictionary<string, object> capabilities, 
            DateTime? lastActivity)
        {
            ActorName = actorName;
            TheadId = theadId;
            Started = started;
            Completed = completed;
            Process = process;
            Capabilities = capabilities;
            LastActivity = lastActivity;
        }

        public string ActorName { get; }
        public int TheadId { get; }
        public int Started { get; }
        public int Completed { get; }
        public ProcessInfo Process { get; }
        public IReadOnlyDictionary<string, object> Capabilities { get; }
        public DateTime? LastActivity { get; }
    }

    public class ProcessInfo
    {
        public ProcessInfo(
            int processId,            
            string processName,
            DateTime startTime,
            CpuTime cpuTime,
            MemoryUsage memoryUsage)
        {
            Id = processId;
            Name = processName;
            StartTime = startTime;
            CpuTime = cpuTime;
            MemoryUsage = memoryUsage;
        }

        public int Id { get; }
        public string Name { get; }
        public DateTime StartTime { get; }
        public CpuTime CpuTime { get; }
        public MemoryUsage MemoryUsage { get; }
    }

    public class CpuTime
    {
        public CpuTime(TimeSpan total, TimeSpan user, TimeSpan privileged)
        {
            Total = total.ToString(@"d\.hh\:mm\:ss\:FFF");
            User = user.ToString(@"d\.hh\:mm\:ss\:FFF");
            Privileged = privileged.ToString(@"d\.hh\:mm\:ss\:FFF");
        }

        public string Total { get; }
        public string User { get; }
        public string Privileged { get; }
    }

    public class MemoryUsage
    {
        public MemoryUsage(long current, long peak, int activeThreads)
        {
            Current = current;
            Peak = peak;
            ActiveThreads = activeThreads;
        }

        public long Current { get; }
        public long Peak { get; }
        public int ActiveThreads { get; }
    }    
}

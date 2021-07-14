namespace PrerenderDotNet.Clients
{
    public interface IProcessClient
    {
        void AddId(int id);
        void DeleteOrphanedProcesses();
        void Clear();
        int[] GetKnownIds();
    }
}
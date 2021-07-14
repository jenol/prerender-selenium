using PrerenderDotNet.Messages;
using System.Threading.Tasks;

namespace PrerenderDotNet.Clients
{
    public interface IBrowserClient
    {
        Task<BrowserInfoResponse> GetBrowserInfoAsync();
    }
}
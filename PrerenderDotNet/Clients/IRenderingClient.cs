using PrerenderDotNet.Messages;
using System.Threading.Tasks;

namespace PrerenderDotNet.Clients
{
    public interface IRenderingClient
    {
        Task<RenderingResponseMessage> RenderAsync(string url, bool isMobile, Activity activity);
    }
}
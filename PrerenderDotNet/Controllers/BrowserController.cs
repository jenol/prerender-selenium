using Microsoft.AspNetCore.Mvc;
using PrerenderDotNet.Clients;
using PrerenderDotNet.Messages;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PrerenderDotNet.Controllers
{
    [ApiController]
    [Route("browsers")]
    public class BrowserController: ControllerBase
    {
        private readonly IBrowserClient browserClient;
        public BrowserController(IBrowserClient browserClient)
        {
            this.browserClient = browserClient;
        }

        [HttpGet]
        public async Task<IEnumerable<BrowserInfo>> Get()
        {
            var response = await browserClient.GetBrowserInfoAsync();
            return response.Browsers;
        }
    }
}

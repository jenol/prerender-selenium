using Microsoft.AspNetCore.Mvc;
using PrerenderDotNet.Clients;
using System.Threading.Tasks;
using PrerenderDotNet.Messages;
using System.IO;

namespace PrerenderDotNet.Controllers
{
    [ApiController]
    [Route("")]
    public class RenderingController : ControllerBase
    {
        private readonly IRenderingClient renderingClient;
        public RenderingController(IRenderingClient renderingClient)
        {
            this.renderingClient = renderingClient;
        }

        [HttpGet]
        [ProducesResponseType(typeof(string), 200)]
        public async Task<IActionResult> Get([FromQuery]string url, [FromQuery] bool isMobile)
        {
            var response = await renderingClient.RenderAsync(url, isMobile, Activity.RenderHtml);
            Response.Headers.Add("X-Actor", response.ActorPath);            

            if (response.RedirectUrl != null)
            {                
                return new RedirectResult(response.RedirectUrl);
            }

            return new ContentResult()
            {
                Content = response.Html ?? "",
                ContentType = "text/html",
                StatusCode = response.HttpStatus,
            };
        }

        [HttpGet]
        [Route("screenshot")]
        [ProducesResponseType(typeof(byte[]), 200)]        
        public async Task<IActionResult> GetScreenshot([FromQuery] string url, [FromQuery] bool isMobile)
        {            
            var response = await renderingClient.RenderAsync(url, isMobile, Activity.Screenshot);
            Response.Headers.Add("X-Actor", response.ActorPath);

            if (response.RedirectUrl != null)
            {
                return new RedirectResult(response.RedirectUrl);
            }         

            return File(response.ScreenShot, "image/jpeg");
        }
    }
}

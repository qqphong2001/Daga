using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using webdaga.Helper;

namespace webdaga.Controllers
{
    [Route("api/live")]
    public class LiveController : Controller
    {
        private readonly StreamOptions _opt;
        public LiveController(IOptions<StreamOptions> opt) {
            _opt = opt.Value;
        }

        [HttpGet("status")]
        public async Task<IActionResult> Status()
        {
            try
            {
                using var client = new HttpClient { Timeout = TimeSpan.FromSeconds(2) };
                var res = await client.SendAsync(new HttpRequestMessage(HttpMethod.Head, _opt.HlsLocalProbeUrl));
                return Ok(new { live = res.IsSuccessStatusCode });
            }
            catch { return Ok(new { live = false }); }
        }

        public IActionResult Index()
        {
            return View();
        }
    }
}

using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using System.Diagnostics;
using webdaga.Helper;
using webdaga.Models;

namespace webdaga.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly StreamOptions _opt;

        public HomeController(ILogger<HomeController> logger, IOptions<StreamOptions> opt, IHttpClientFactory? httpFactory = null)
        {
            _logger = logger;
            _opt = opt.Value;
        }

        public async Task<IActionResult> Index()
        {
            ViewBag.HlsUrl = _opt.HlsPublicUrl;
            ViewBag.IsLive = await ProbeLiveAsync();
            return View();
        }
        private async Task<bool> ProbeLiveAsync()
        {
            try
            {
                using var client = new HttpClient();
                client.Timeout = TimeSpan.FromSeconds(2);
                using var req = new HttpRequestMessage(HttpMethod.Head, _opt.HlsLocalProbeUrl);
                var res = await client.SendAsync(req);
                return res.IsSuccessStatusCode;
            }
            catch { return false; }
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}

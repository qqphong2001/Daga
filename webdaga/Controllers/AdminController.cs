using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using webdaga.Helper;

namespace webdaga.Controllers
{

    public class AdminController : Controller
    {
        private readonly StreamOptions _opt;
        public AdminController(IOptions<StreamOptions> opt)
        {
            _opt = opt.Value;
        }
        [HttpGet]
        public IActionResult Publish()
        {
            ViewBag.WhipEndpoint = _opt.WhipEndpoint;
            ViewBag.WhipBearerToken = _opt.WhipBearerToken;
            return View();
        }
        public IActionResult Index()
        {
            return View();
        }
    }
}

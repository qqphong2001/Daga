using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using webdaga.Helper;

namespace webdaga.Controllers
{
    [Authorize]
    public class AdminController : Controller
    {
        private readonly StreamOptions _opt;
        public AdminController(IOptions<StreamOptions> opt)
        {
            _opt = opt.Value;
        }
        [Authorize]
        [HttpGet]
        public IActionResult Publish()
        {
            ViewBag.WhipEndpoint = _opt.WhipEndpoint;
            ViewBag.WhipBearerToken = _opt.WhipBearerToken;
            ViewBag.Admin = _opt.Admin;
            ViewBag.AdminPassword = _opt.AdminPassword;
            ViewBag.User = _opt.User;
            ViewBag.UserPassword = _opt.UserPassword;
            return View();
        }
        public IActionResult Index()
        {
            return View();
        }
    }
}

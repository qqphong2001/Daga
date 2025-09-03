using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using System.Diagnostics;
using System.Drawing.Printing;
using webdaga.DbContext;
using webdaga.Helper;
using webdaga.Models;

namespace webdaga.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly StreamOptions _opt;
        private readonly ApplicationDbContext _db;

        public HomeController(ILogger<HomeController> logger, ApplicationDbContext db, IOptions<StreamOptions> opt, IHttpClientFactory? httpFactory = null)
        {
            _logger = logger;
            _opt = opt.Value;
            _db = db;
        }

        public async Task<IActionResult> Index()
        {
            ViewBag.streanUrl = _opt.streamUrl;
            ViewBag.User = _opt.User;
            ViewBag.UserPassword = _opt.UserPassword;
            var Articles = _db.Articles
            .OrderByDescending(a => a.CreatedDate)
            .Skip((1 - 1) * 3)
            .Take(3)
            .ToList();
            return View(Articles);
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

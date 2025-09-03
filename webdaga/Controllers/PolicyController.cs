using Microsoft.AspNetCore.Mvc;

namespace webdaga.Controllers
{
    public class PolicyController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}

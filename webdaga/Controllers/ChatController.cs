using Microsoft.AspNetCore.Mvc;

namespace webdaga.Controllers
{
    public class ChatController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}

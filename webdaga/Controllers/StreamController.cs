using Microsoft.AspNetCore.Mvc;

namespace webdaga.Controllers
{
    public class StreamController : Controller
    {
        private readonly IWebHostEnvironment _environment;

        public StreamController(IWebHostEnvironment environment)
        {
            _environment = environment;
        }

        public IActionResult Index()
        {
            // Viewer page
            return View();
        }

        public IActionResult Admin()
        {
            // Admin streaming page
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> SaveVideo(IFormFile video)
        {
            if (video != null)
            {
                var uploadsFolder = Path.Combine(_environment.WebRootPath, "videos");
                Directory.CreateDirectory(uploadsFolder);
                var fileName = $"{Guid.NewGuid()}.mp4";
                var filePath = Path.Combine(uploadsFolder, fileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await video.CopyToAsync(stream);
                }

                return Ok(new { path = $"/videos/{fileName}" });
            }
            return BadRequest();
        }

        public IActionResult GetVideos()
        {
            var uploadsFolder = Path.Combine(_environment.WebRootPath, "videos");
            if (!Directory.Exists(uploadsFolder))
                return Json(new string[] { });

            var videos = Directory.GetFiles(uploadsFolder)
                .Select(f => "/videos/" + Path.GetFileName(f))
                .ToArray();
            return Json(videos);
        }
    }
}

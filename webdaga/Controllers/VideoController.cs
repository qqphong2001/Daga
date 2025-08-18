using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using webdaga.Helper;
using System.IO;

namespace webdaga.Controllers
{
    public class VideoController : Controller
    {
        private readonly StreamOptions _opt;

        public VideoController(IOptions<StreamOptions> opt)
        {
            _opt = opt.Value;
        }

        public IActionResult Index()
        {
            var list = new List<VodItem>();
            if (!string.IsNullOrWhiteSpace(_opt.VodPhysicalPath) && Directory.Exists(_opt.VodPhysicalPath))
            {
                foreach (var file in Directory.EnumerateFiles(_opt.VodPhysicalPath, "*.mp4", SearchOption.TopDirectoryOnly)
                    .OrderByDescending(System.IO.File.GetCreationTimeUtc))
                {
                    var name = Path.GetFileName(file);
                    list.Add(new VodItem
                    {
                        Name = Path.GetFileNameWithoutExtension(name),
                        Url = CombineUrl(_opt.VodPublicBaseUrl ?? "/vod", name),
                        SizeBytes = new FileInfo(file).Length,
                        CreatedUtc = System.IO.File.GetCreationTimeUtc(file)
                    });
                }
            }
            return View(list);
        }

        // New action to serve a video file
        public IActionResult Stream(string fileName)
        {
            if (string.IsNullOrWhiteSpace(fileName))
            {
                return BadRequest("File name is required.");
            }

            var filePath = Path.Combine(_opt.VodPhysicalPath, fileName);
            if (!System.IO.File.Exists(filePath))
            {
                return NotFound("Video file not found.");
            }

            // Open the file stream
            var fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read);

            // Return the file with the appropriate content type for MP4
            return File(fileStream, "video/mp4", enableRangeProcessing: true); // Supports video streaming
        }

        private static string CombineUrl(string baseUrl, string path)
            => string.Concat((baseUrl?.TrimEnd('/') ?? string.Empty), "/", path.TrimStart('/'));
    }
}

public sealed class VodItem
{
    public string Name { get; set; } = string.Empty;
    public string Url { get; set; } = string.Empty;
    public long SizeBytes { get; set; }
    public DateTime CreatedUtc { get; set; }
}
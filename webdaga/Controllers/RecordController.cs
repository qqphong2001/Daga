using Microsoft.AspNetCore.Mvc;
using System.Collections.Concurrent;
using System.ComponentModel.DataAnnotations;

namespace webdaga.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class RecordController : ControllerBase
    {
        private static readonly ConcurrentDictionary<string, RecordEvent> _recordedFiles = new();
        private readonly ILogger<RecordController> _logger;

        public RecordController(ILogger<RecordController> logger)
        {
            _logger = logger;
        }

        [HttpPost("record-complete")]
        public IActionResult RecordComplete([FromQuery] string path, [FromQuery] string segment_path)
        {
            // Kiểm tra dữ liệu đầu vào
            if (string.IsNullOrEmpty(path) || string.IsNullOrEmpty(segment_path))
            {
                _logger.LogWarning("Received invalid or empty query parameters.");
                return BadRequest("Path and segment_path are required.");
            }

            var recordEvent = new RecordEvent
            {
                Path = path,
                File = segment_path,
                Created = DateTime.UtcNow
            };

            // Lưu file mới nhất
            _recordedFiles["latest"] = recordEvent;
            _logger.LogInformation($"Received new recording: Path={path}, SegmentPath={segment_path}");

            return Ok(new { Message = "Record processed successfully", Path = path, SegmentPath = segment_path });
        }

        [HttpGet("latest")]
        public IActionResult GetLatestFile()
        {
            if (!_recordedFiles.TryGetValue("latest", out var latestFile) || latestFile == null)
            {
                _logger.LogWarning("No recorded file found.");
                return NotFound("No recorded file available.");
            }

            return Ok(new
            {
                latestFile.Path,
                latestFile.File,
                latestFile.Created
            });
        }
    }

    public class RecordEvent
    {
        [Required(ErrorMessage = "Path is required")]
        public string Path { get; set; }

        [Required(ErrorMessage = "File is required")]
        public string File { get; set; }

        public DateTime Created { get; set; }
    }
}
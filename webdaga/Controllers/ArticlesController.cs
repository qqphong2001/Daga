using Microsoft.AspNetCore.Mvc;
using webdaga.DbContext;
using webdaga.Models;

namespace webdaga.Controllers
{
    public class ArticlesController : Controller
    {
        private readonly ApplicationDbContext _db;
        private readonly IConfiguration _configuration;
        public ArticlesController(ApplicationDbContext db, IConfiguration configuration)
        {
            _db = db;
            _configuration = configuration;
        }
        public IActionResult Index(int page = 1)
        {
            int pageSize = 6;
            var totalArticles = _db.Articles.Count();
            var totalPages = (int)Math.Ceiling(totalArticles / (double)pageSize);

            var articles = GetArticles(page, pageSize);

            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = totalPages;

            return View(articles);
        }

        public List<articlesModel> GetArticles(int pageNumber, int pageSize)
        {
            return _db.Articles
                .OrderByDescending(a => a.CreatedDate)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToList();
        }

        public IActionResult Details(int id)
        {
            var article = _db.Articles.FirstOrDefault(a => a.Id == id);
            if (article == null)
            {
                return NotFound();
            }
            return View(article);
        }

        public IActionResult SaveArticles()
        {
            string folderPath = _configuration["Stream:RecordPath"];

            try
            {
                if (!Directory.Exists(folderPath))
                    return NotFound("Thư mục không tồn tại.");

                var mp4Files = Directory.GetFiles(folderPath, "*.mp4")
                                        .Select(Path.GetFileName)
                                        .ToList();

                if (mp4Files.Count == 0)
                    return NotFound("Không có file .mp4 nào.");

                // Lấy file mới nhất và parse ngày
                var newest = mp4Files
                    .Select(name =>
                    {
                        var ok = DateTime.TryParseExact(
                            Path.GetFileNameWithoutExtension(name),
                            "yyyy-MM-dd_HH-mm-ss-ffffff",
                            null,
                            System.Globalization.DateTimeStyles.None,
                            out var date
                        );
                        return new { Name = name, Date = ok ? date : DateTime.MinValue };
                    })
                    .OrderByDescending(x => x.Date)
                    .FirstOrDefault();

                if (newest == null || newest.Date == DateTime.MinValue)
                    return NotFound("Không có file hợp lệ.");

                // Format dd/MM/yyyy
                string formattedDate = newest.Date.ToString("dd/MM/yyyy");

                // Lưu vào DB (ví dụ)
                _db.Articles.Add(new articlesModel()
                {
                    Name = "Video Vần xổ gà " + formattedDate + " – CLB Gà Chọi C3",
                    CreatedDate = newest.Date,
                    CreatedBy = "C3",
                    Description = "",
                    ImgUrl =  Url.Content($"~/gachoi.jpg"),
                    VideoUrl = Url.Content($"~/recordings/mystream/{newest.Name}"),
                    UpdatedBy = "C3",
                });

                _db.SaveChanges();

                return Ok(new { newest.Name, FormattedDate = formattedDate });
            }
            catch (Exception)
            {
                throw;
            }
        }
    }
}

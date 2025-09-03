using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Linq;
using webdaga.DbContext;
using webdaga.Models;

namespace webdaga.admin.Controllers
{
    [Area("admin")]
    [Authorize]

    public class ArticlesController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ArticlesController(ApplicationDbContext context)
        {
            _context = context;
        }

        // Danh sách + tìm kiếm
        public IActionResult Index(string searchString, int page = 1, int pageSize = 5)
        {
            var articles = _context.Articles
                                   .OrderByDescending(a => a.CreatedDate)
                                   .AsQueryable();

            if (!string.IsNullOrEmpty(searchString))
            {
                articles = articles.Where(s => s.Name.Contains(searchString));
            }

            var totalItems = articles.Count();

            var pagedArticles = articles
                                .Skip((page - 1) * pageSize)
                                .Take(pageSize)
                                .ToList();

            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = (int)Math.Ceiling(totalItems / (double)pageSize);
            ViewBag.SearchString = searchString;

            return View("~/Areas/admin/Views/Articles/Index.cshtml", pagedArticles);
        }




        // GET: Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Create
 
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(articlesModel article)
        {

            try
            {
                article.CreatedDate = DateTime.Now;
                article.UpdatedDate = DateTime.Now;

                article.CreatedBy = User.Identity.Name;
                article.UpdatedBy = User.Identity.Name;

                _context.Add(article);
                _context.SaveChanges();
                return RedirectToAction(nameof(Index));
            }
            catch (Exception)
            {

                return View(article);

            }


        }

        // GET: Edit
        public IActionResult Edit(int id)
        {
            var article = _context.Articles.Find(id);
            if (article == null) return NotFound();
            return View(article);
        }

        // POST: Edit

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(articlesModel article)
        {
            try
            {
                var existing = _context.Articles.FirstOrDefault(x => x.Id == article.Id);
                if (existing == null) return NotFound();

                // Cập nhật từng trường thay đổi
                existing.Name = article.Name;
                existing.Description = article.Description;
                existing.ImgUrl = article.ImgUrl;
                existing.VideoUrl = article.VideoUrl;

                // Giữ nguyên CreatedDate, CreatedBy
                existing.UpdatedBy = User.Identity?.Name;
                existing.UpdatedDate = DateTime.Now;

                _context.SaveChanges();
                return RedirectToAction(nameof(Index));
            }
            catch (Exception)
            {
                return View(article);
            }
        }


        // GET: Delete
        public IActionResult Delete(int id)
        {
            var article = _context.Articles.Find(id);
            if (article == null) return NotFound();
            return View(article);
        }

        // POST: Delete
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteConfirmed(int id)
        {
            var article = _context.Articles.Find(id);
            _context.Articles.Remove(article);
            _context.SaveChanges();
            return RedirectToAction(nameof(Index));
        }
    }
}

using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MiniCMS.Web.Data;
using MiniCMS.Web.Models;
using MiniCMS.Web.Services;
using MiniCMS.Web.Areas.Admin.Models;

namespace MiniCMS.Web.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class ArticlesController : Controller
    {
        private readonly ApplicationDbContext _db;
        private readonly IHtmlSanitizationService _sanitizer;

        public ArticlesController(ApplicationDbContext db, IHtmlSanitizationService sanitizer)
        {
            _db = db;
            _sanitizer = sanitizer;
        }

        // GET: /Admin/Articles
        public async Task<IActionResult> Index()
        {
            var articles = await _db.Articles
                .OrderByDescending(a => a.UpdatedAt)
                .ToListAsync();

            return View(articles);
        }

        // GET: /Admin/Articles/Details/{id}
        public async Task<IActionResult> Details(int id)
        {
            var article = await _db.Articles.FirstOrDefaultAsync(a => a.Id == id);
            if (article == null) return NotFound();

            return View(article);
        }

        // GET: /Admin/Articles/Create
        public IActionResult Create()
        {
            return View(new ArticleEditViewModel());
        }

        // POST: /Admin/Articles/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ArticleEditViewModel vm)
        {
            if (!ModelState.IsValid)
                return View(vm);

            var sanitizedContent = _sanitizer.Sanitize(vm.Content ?? string.Empty);

            var article = new Article
            {
                Title = vm.Title.Trim(),
                Content = sanitizedContent
                // CreatedAt/UpdatedAt are handled by DbContext SaveChanges override
            };

            _db.Articles.Add(article);
            await _db.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        // GET: /Admin/Articles/Edit/{id}
        public async Task<IActionResult> Edit(int id)
        {
            var article = await _db.Articles.FirstOrDefaultAsync(a => a.Id == id);
            if (article == null) return NotFound();

            var vm = new ArticleEditViewModel
            {
                Id = article.Id,
                Title = article.Title,
                Content = article.Content
            };

            return View(vm);
        }

        // POST: /Admin/Articles/Edit/{id}
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, ArticleEditViewModel vm)
        {
            if (id != vm.Id)
                return NotFound();

            if (!ModelState.IsValid)
                return View(vm);

            var article = await _db.Articles.FirstOrDefaultAsync(a => a.Id == id);
            if (article == null) return NotFound();

            article.Title = vm.Title.Trim();
            article.Content = _sanitizer.Sanitize(vm.Content ?? string.Empty);
            // UpdatedAt handled by DbContext SaveChanges override

            await _db.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        // GET: /Admin/Articles/Delete/{id}
        public async Task<IActionResult> Delete(int id)
        {
            var article = await _db.Articles.FirstOrDefaultAsync(a => a.Id == id);
            if (article == null) return NotFound();

            return View(article);
        }

        // POST: /Admin/Articles/Delete/{id}
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var article = await _db.Articles.FirstOrDefaultAsync(a => a.Id == id);
            if (article == null) return NotFound();

            _db.Articles.Remove(article);
            await _db.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }
    }
}
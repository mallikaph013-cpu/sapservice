using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using myapp.Data;
using myapp.Models;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using System.IO;

namespace myapp.Controllers
{
    [Authorize]
    public class NewsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _environment;

        public NewsController(ApplicationDbContext context, IWebHostEnvironment environment)
        {
            _context = context;
            _environment = environment;
        }

        // GET: News
        [Authorize(Roles = "IT")]
        public async Task<IActionResult> Index()
        {
            var articles = await _context.NewsArticles.OrderByDescending(a => a.PublishedDate).ToListAsync();
            return View(articles);
        }

        // GET: News/Details/5
        public async Task<IActionResult> Details(int? id)
        {   
            if (id == null)
            {
                return NotFound();
            }

            var article = await _context.NewsArticles
                .FirstOrDefaultAsync(m => m.Id == id);
            if (article == null)
            {
                return NotFound();
            }

            return View(article);
        }

        // GET: News/Create
        [Authorize(Roles = "IT")]
        public IActionResult Create()
        {
            return View();
        }

        // POST: News/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "IT")]
        public async Task<IActionResult> Create(NewsArticle article, IFormFile? imageFile)
        {
            if (ModelState.IsValid)
            {
                if (imageFile != null && imageFile.Length > 0)
                {
                    var uploadsDirectory = Path.Combine(_environment.WebRootPath, "uploads", "news");
                    Directory.CreateDirectory(uploadsDirectory);

                    var extension = Path.GetExtension(imageFile.FileName);
                    var fileName = $"{Guid.NewGuid():N}{extension}";
                    var filePath = Path.Combine(uploadsDirectory, fileName);

                    await using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await imageFile.CopyToAsync(stream);
                    }

                    article.ImageUrl = $"/uploads/news/{fileName}";
                }

                _context.Add(article);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Article created successfully!";
                return RedirectToAction(nameof(Index));
            }
            return View(article);
        }

        // GET: News/Edit/5
        [Authorize(Roles = "IT")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var article = await _context.NewsArticles.FindAsync(id);
            if (article == null)
            {
                return NotFound();
            }
            return View(article);
        }

        // POST: News/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "IT")]
        public async Task<IActionResult> Edit(int id, NewsArticle article)
        {
            if (id != article.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(article);
                    await _context.SaveChangesAsync();
                     TempData["SuccessMessage"] = "Article updated successfully!";
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!await ArticleExists(article.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            return View(article);
        }

        // GET: News/Delete/5
        [Authorize(Roles = "IT")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var article = await _context.NewsArticles
                .FirstOrDefaultAsync(m => m.Id == id);
            if (article == null)
            {
                return NotFound();
            }

            return View(article);
        }

        // POST: News/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "IT")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var article = await _context.NewsArticles.FindAsync(id);
            if (article != null) {
                 _context.NewsArticles.Remove(article);
                 await _context.SaveChangesAsync();
                 TempData["SuccessMessage"] = "Article deleted successfully!";
            }
            return RedirectToAction(nameof(Index));
        }

        private async Task<bool> ArticleExists(int id)
        {
            return await _context.NewsArticles.AnyAsync(e => e.Id == id);
        }
    }
}

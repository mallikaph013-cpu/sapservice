using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using myapp.Models;
using System.Diagnostics;
using System.Linq;
using myapp.Data;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;

namespace myapp.Controllers
{
    [Authorize]
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly ApplicationDbContext _context;

        public HomeController(ILogger<HomeController> logger, ApplicationDbContext context)
        {
            _logger = logger;
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var articles = await _context.NewsArticles
                                     .OrderByDescending(a => a.PublishedDate)
                                     .Select(a => new NewsArticleViewModel
                                     {
                                         Id = a.Id,
                                         IsFeatured = a.IsFeatured,
                                         Title = a.Title,
                                         Excerpt = a.Content.Substring(0, Math.Min(a.Content.Length, 150)) + "...", // Create a short excerpt
                                         ImageUrl = a.ImageUrl,
                                         PublishedDate = a.PublishedDate,
                                         Author = a.Author,
                                         ArticleUrl = "/News/Details/" + a.Id
                                     })
                                     .ToListAsync();

            return View(articles);
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

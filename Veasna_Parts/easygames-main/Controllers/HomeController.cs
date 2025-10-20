using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using EasyGames.Data;
using EasyGames.Models;

namespace EasyGames.Controllers
{
    public class HomeController : Controller
    {
        private readonly ApplicationDbContext _db;
        public HomeController(ApplicationDbContext db) => _db = db;

        // Landing is reached by Program.cs root route and by /Home/Landing
        public IActionResult Landing() => View("Landing");

        // Store with search/filter by enum Category notes on readme
        public async Task<IActionResult> Index(string? q, string? category)
        {
            ViewBag.Q = q ?? string.Empty;
            ViewBag.Category = category ?? string.Empty;

            var categories = await _db.Products
                .AsNoTracking()
                .Select(p => p.Category)
                .Distinct()
                .OrderBy(c => c)
                .ToListAsync();
            ViewBag.Categories = categories;

            var query = _db.Products.AsNoTracking();

            if (!string.IsNullOrWhiteSpace(q))
            {
                var term = q.Trim();
                query = query.Where(p => p.Title != null && EF.Functions.Like(p.Title, $"%{term}%"));
            }

            if (!string.IsNullOrWhiteSpace(category) &&
                Enum.TryParse<Category>(category, true, out var cat))
            {
                query = query.Where(p => p.Category == cat);
            }

            var products = await query.OrderBy(p => p.Title).ToListAsync();
            return View(products);
        }

        public IActionResult Privacy() => View();

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel
            {
                RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier
            });
        }
    }
}


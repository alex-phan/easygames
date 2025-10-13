using EasyGames.Data;            // ApplicationDbContext
using EasyGames.Filters;         // AuthorizeOwner
using EasyGames.Models;          // Product and  Category
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace EasyGames.Areas.Admin.Controllers
{
    [Area("Admin")]
    [AuthorizeOwner] // lock to Owner role; alt: [Authorize(Roles = "Owner")]
    public class ProductsController : Controller
    {
        private readonly ApplicationDbContext _db;

        public ProductsController(ApplicationDbContext db) => _db = db;

        // GET: /Admin/Products
        [HttpGet]
        public async Task<IActionResult> Index(string? q)
        {
            var query = _db.Products.AsNoTracking().AsQueryable();

            if (!string.IsNullOrWhiteSpace(q))
            {
                q = q.Trim();
                var pattern = $"%{q}%";

                // Try to interpret q as a Category enum name (Book, Game, Toy)
                if (Enum.TryParse<Category>(q, ignoreCase: true, out var parsedCat))
                {
                    // Title LIKE OR exact category enum match (translated to SQL )
                    query = query.Where(p =>
                        EF.Functions.Like(p.Title, pattern) ||
                        p.Category == parsedCat);
                }
                else
                {
                    // Only Title LIKE when q is not a known category name
                    query = query.Where(p => EF.Functions.Like(p.Title, pattern));
                }
            }

            var items = await query
                .OrderBy(p => p.Id)
                .ToListAsync();

            // Index.cshtml  accepts a plain List<Product>
            return View(items);
        }

        // GET for : /Admin/Products/Create
        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }

        // POST: /Admin/Products/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Product input)
        {
            if (!ModelState.IsValid) return View(input);

            _db.Products.Add(input);
            await _db.SaveChangesAsync();

            TempData["Toast"] = "Product created.";
            return RedirectToAction(nameof(Index));
        }

        // GET: /Admin/Products/Edit/5
        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var product = await _db.Products.FindAsync(id);
            if (product == null) return NotFound();
            return View(product);
        }

        // POST: /Admin/Products/Edit/5 // update fields via modified state email/password/rememberMe/returnUrl got ChatGpt help 
        // repo pattern notes recorded in Notepad repo1 (avoid overposting, no AsNoTracking for update)

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Product input)
        {
            if (id != input.Id) return BadRequest();
            if (!ModelState.IsValid) return View(input);

            var exists = await _db.Products.AnyAsync(p => p.Id == id);
            if (!exists) return NotFound();

            _db.Entry(input).State = EntityState.Modified;
            await _db.SaveChangesAsync();

            TempData["Toast"] = "Product updated.";
            return RedirectToAction(nameof(Index));
        }

        // POST: /Admin/Products/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var product = await _db.Products.FindAsync(id);
            if (product == null) return NotFound();

            _db.Products.Remove(product);
            await _db.SaveChangesAsync();

            TempData["Toast"] = "Product deleted.";
            return RedirectToAction(nameof(Index));
        }
    }
}




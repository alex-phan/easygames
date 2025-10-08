// admin orders list + details (quick view for totals/profit) see Notepad n3.e
using EasyGames.Data;
using EasyGames.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EasyGames.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class OrdersController(ApplicationDbContext db) : Controller
    {
        private readonly ApplicationDbContext _db = db;

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            // pull orders with user + item count for quick admin scan
            var orders = await _db.Orders
                .Include(o => o.User)
                .Include(o => o.Items)
                .OrderByDescending(o => o.Id)
                .ToListAsync();

            return View(orders);
        }

        [HttpGet]
        public async Task<IActionResult> Details(int id)
        {
            // include items + products for per-line pricing/cost snapshot
            var order = await _db.Orders
                .Include(o => o.User)
                .Include(o => o.Items)
                    .ThenInclude(i => i.Product)
                .FirstOrDefaultAsync(o => o.Id == id);

            if (order == null) return NotFound();

            return View(order);
        }

        // ---- status update (Placed -> Paid -> Shipped) ----
        // POST + antiforgery; redirects back to Details
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateStatus(int id, string status)
        {
            var order = await _db.Orders.FirstOrDefaultAsync(o => o.Id == id);
            if (order == null) return NotFound();

            // accept either explicit status (Placed/Paid/Shipped) or "next"
            if (string.Equals(status, "next", StringComparison.OrdinalIgnoreCase))
            {
                order.Status = order.Status switch
                {
                    OrderStatus.Placed => OrderStatus.Paid,
                    OrderStatus.Paid => OrderStatus.Shipped,
                    _ => OrderStatus.Shipped
                };
            }
            else if (Enum.TryParse<OrderStatus>(status, ignoreCase: true, out var parsed))
            {
                order.Status = parsed;
            }
            else
            {
                TempData["Toast"] = "Invalid status.";
                return RedirectToAction(nameof(Details), new { id });
            }

            await _db.SaveChangesAsync();
            TempData["Toast"] = $"Order status updated to {order.Status}.";
            return RedirectToAction(nameof(Details), new { id });
        }
    }
}


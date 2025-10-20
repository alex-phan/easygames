// simple orders history page + quick tier calc (customer) See Readme
using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using EasyGames.Data;

namespace EasyGames.Controllers
{
    public class OrdersController(ApplicationDbContext db) : Controller
    {
        private readonly ApplicationDbContext _db = db;

        [HttpGet]
        public async Task<IActionResult> My()
        {
            // must be signed in to see orders
            if (!(User.Identity?.IsAuthenticated ?? false))
                return RedirectToAction("Login", "Account", new { returnUrl = Url.Action(nameof(My), "Orders") });

            // get current user id (cookie claim added by auth)
            var idValue = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrWhiteSpace(idValue) || !int.TryParse(idValue, out var userId))
                return RedirectToAction("Logout", "Account");

            // pull orders with items + products for display
            var orders = await _db.Orders
                .Where(o => o.UserId == userId)
                .Include(o => o.Items)
                    .ThenInclude(i => i.Product)
                .OrderByDescending(o => o.Id)
                .ToListAsync();

            // quick lifetime profit -> tier (temporary here; move to service later)
            var lifetimeProfit = orders.Sum(o => o.Profit);
            var tier = lifetimeProfit switch
            {
                < 100m => "Bronze",
                < 500m => "Silver",
                < 2000m => "Gold",
                _ => "Platinum"
            };

            ViewBag.Tier = tier;
            ViewBag.LifetimeProfit = lifetimeProfit;

            return View(orders);
        }
    }
}

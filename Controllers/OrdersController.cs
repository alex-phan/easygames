// simple orders history page + quick tier calc (customer) See Readme
using EasyGames.Data;
using EasyGames.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace EasyGames.Controllers
{
    public class OrdersController(ApplicationDbContext db, ITierService tiers) : Controller
    {
        private readonly ApplicationDbContext _db = db;
        private readonly ITierService _tiers = tiers;

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

            // define tier + lifetime profit by using the tier service (Alex Phan)
            var (tier, lifetimeProfit) = await _tiers.GetTierForUserAsync(userId);

            ViewBag.Tier = tier;
            ViewBag.LifetimeProfit = lifetimeProfit;

            return View(orders);
        }
    }
}

// checkout controller: cart -> place order -> thank-you (with totals)  // small polish + fallback Notes to Team 
using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using EasyGames.Data;
using EasyGames.Services;
using EasyGames.ViewModels;

namespace EasyGames.Controllers
{
    public class CheckoutController(ICartService cart, ICheckoutService checkout, ApplicationDbContext db) : Controller
    {
        private readonly ICartService _cart = cart;
        private readonly ICheckoutService _checkout = checkout;
        private readonly ApplicationDbContext _db = db;

        [HttpGet]
        public IActionResult Index()
        {
            var vm = new CheckoutVM
            {
                FullName = User.Identity?.IsAuthenticated == true ? (User.Identity?.Name ?? "") : "",
                Email = User.FindFirstValue(ClaimTypes.Email) ?? ""
            };

            if (_cart.Count() == 0)
            {
                TempData["Toast"] = "Your cart is empty.";
                return RedirectToAction("Index", "Cart");
            }

            vm.Subtotal = _cart.Total();
            vm.Items = _cart.Items();
            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Index(CheckoutVM vm)
        {
            if (!ModelState.IsValid)
            {
                vm.Subtotal = _cart.Total();
                vm.Items = _cart.Items();
                return View(vm);
            }

            if (_cart.Count() == 0)
            {
                TempData["Toast"] = "Your cart is empty.";
                return RedirectToAction("Index", "Cart");
            }

            // must be logged in to place an order (keeps it simple for now)
            if (!(User.Identity?.IsAuthenticated ?? false))
            {
                TempData["Toast"] = "Please sign in to complete checkout.";
                return RedirectToAction("Login", "Account", new { returnUrl = Url.Action(nameof(Index), "Checkout") });
            }

            // resolve current user id (assumes cookie contains NameIdentifier claim set by AuthService)
            var idValue = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrWhiteSpace(idValue) || !int.TryParse(idValue, out var userId))
            {
                TempData["Toast"] = "Could not resolve current user. Please sign in again.";
                return RedirectToAction("Logout", "Account");
            }

            // save order + reduce stock (transactional) 
            var lines = _cart.Items()
                             .Select(ci => new OrderLineInput(ci.ProductId, ci.Qty))
                             .ToList();

            // simple: no tax calc yet (0); can switch to %-based later
            var order = await _checkout.PlaceOrderAsync(userId, lines, tax: 0m);

            // friendly order number using db id
            var orderNo = $"EG-{order.Id:000000}";

            _cart.Clear();

            TempData["OrderNo"] = orderNo;
            TempData["OrderName"] = vm.FullName ?? "";
            TempData["OrderEmail"] = vm.Email ?? "";

            // totals for thank-you page (snapshot on order)  // small polish here
            TempData["OrderSubtotal"] = order.Subtotal.ToString("C");
            TempData["OrderTax"] = order.Tax.ToString("C");
            TempData["OrderTotal"] = order.Total.ToString("C");

            return RedirectToAction(nameof(Done));
        }

        [HttpGet]
        public async Task<IActionResult> Done()
        {
            // values from TempData if we just placed an order
            var orderNo = TempData["OrderNo"] as string;
            ViewBag.OrderNo = orderNo ?? "EG-UNKNOWN";
            ViewBag.OrderName = TempData["OrderName"] as string ?? "Customer";
            ViewBag.OrderEmail = TempData["OrderEmail"] as string ?? "";
            ViewBag.OrderSubtotal = TempData["OrderSubtotal"] as string ?? "";
            ViewBag.OrderTax = TempData["OrderTax"] as string ?? "";
            ViewBag.OrderTotal = TempData["OrderTotal"] as string ?? "";

            // fallback: if TempData missing (page refreshed or opened directly), load latest order for current user  // see Notepad n3.f.a
            if (orderNo is null && (User.Identity?.IsAuthenticated ?? false))
            {
                var idValue = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (!string.IsNullOrWhiteSpace(idValue) && int.TryParse(idValue, out var userId))
                {
                    var latest = await _db.Orders
                        .Where(o => o.UserId == userId)
                        .OrderByDescending(o => o.Id)
                        .FirstOrDefaultAsync();

                    if (latest != null)
                    {
                        ViewBag.OrderNo = $"EG-{latest.Id:000000}";
                        // leave name/email as-is (from cookie); totals from snapshot
                        ViewBag.OrderSubtotal = latest.Subtotal.ToString("C");
                        ViewBag.OrderTax = latest.Tax.ToString("C");
                        ViewBag.OrderTotal = latest.Total.ToString("C");
                    }
                }
            }

            return View();
        }
    }
}



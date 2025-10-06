using System;
using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using EasyGames.Services;
using EasyGames.ViewModels;

namespace EasyGames.Controllers
{
    public class CheckoutController(ICartService cart) : Controller
    {
        private readonly ICartService _cart = cart;

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
        public IActionResult Index(CheckoutVM vm)
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

            var orderNo = $"EG-{DateTime.UtcNow:yyyyMMdd}-{Random.Shared.Next(1000, 9999)}";

            _cart.Clear();

            TempData["OrderNo"] = orderNo;
            TempData["OrderName"] = vm.FullName ?? "";
            TempData["OrderEmail"] = vm.Email ?? "";

            return RedirectToAction(nameof(Done));
        }

        [HttpGet]
        public IActionResult Done()
        {
            ViewBag.OrderNo = TempData["OrderNo"] as string ?? "EG-UNKNOWN";
            ViewBag.OrderName = TempData["OrderName"] as string ?? "Customer";
            ViewBag.OrderEmail = TempData["OrderEmail"] as string ?? "";
            return View();
        }
    }
}



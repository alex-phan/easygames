using Microsoft.AspNetCore.Mvc;
using EasyGames.Services;
using EasyGames.ViewModels;

namespace EasyGames.Controllers
{
    // Public, customer-facing cart
    public class CartController(ICartService cart) : Controller
    {
        private readonly ICartService _cart = cart;

        // GET /Cart
        [HttpGet]
        public IActionResult Index()
        {
            var items = _cart.Items();
            var vm = new CartPageVM
            {
                Items = items,
                Count = _cart.Count(),
                Subtotal = _cart.Total()
            };
            return View(vm); // /Views/Cart/Index.cshtml
        }

        // POST /Cart/Add
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Add(int productId, int qty = 1)
        {
            _cart.Add(productId, qty);
            TempData["Toast"] = "Added to cart!";
            return RedirectToAction(nameof(Index));
        }

        // POST /Cart/Increase
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Increase(int productId)
        {
            _cart.Increase(productId);
            return RedirectToAction(nameof(Index));
        }

        // POST /Cart/Decrease
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Decrease(int productId)
        {
            _cart.Decrease(productId);
            return RedirectToAction(nameof(Index));
        }

        // POST /Cart/Remove
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Remove(int productId)
        {
            _cart.Remove(productId);
            return RedirectToAction(nameof(Index));
        }

        // POST /Cart/Clear
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Clear()
        {
            _cart.Clear();
            TempData["Toast"] = "Cart cleared.";
            return RedirectToAction(nameof(Index));
        }
    }
}


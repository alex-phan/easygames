using Microsoft.AspNetCore.Mvc;
using EasyGames.Services;

namespace EasyGames.ViewComponents
{
    // Renders the nav Cart link with live count badge.
    public class CartSummaryViewComponent : ViewComponent
    {
        private readonly ICartService _cart;

        public CartSummaryViewComponent(ICartService cart)
        {
            _cart = cart;
        }

        public IViewComponentResult Invoke()
        {
            // Robust: adapt to cart service via the compat extensions.
            int count = 0;
            try { count = _cart.CountCompat(); } catch { count = 0; }
            return View(count); // looks for: /Views/Shared/Components/CartSummary/Default.cshtml
        }
    }
}

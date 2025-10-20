using System.Collections.Generic;

namespace EasyGames.ViewModels
{
    // Cart page view model used by /Views/Cart/Index.cshtml
    public class CartPageVM
    {
        // Must match ICartService.Items()
        public List<CartItemVM> Items { get; set; } = new();

        // Convenience totals for the view
        public int Count { get; set; }
        public decimal Subtotal { get; set; }
    }
}

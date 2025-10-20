using System.Collections.Generic;

namespace EasyGames.ViewModels
{
    public class CartVM
    {
        public List<CartLineVM> Items { get; set; } = new();
        public decimal Subtotal { get; set; }
        public int TotalQty { get; set; }
        public bool IsEmpty => TotalQty == 0;
    }

    public class CartLineVM
    {
        public int ProductId { get; set; }
        public string Title { get; set; } = "";
        public string Category { get; set; } = "";
        public decimal Price { get; set; }
        public int Qty { get; set; }
        public decimal LineTotal => Price * Qty;
    }
}

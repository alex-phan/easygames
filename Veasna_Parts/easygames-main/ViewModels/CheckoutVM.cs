using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace EasyGames.ViewModels
{
    public class CheckoutVM
    {
        // Summary
        public decimal Subtotal { get; set; }
        public List<CartItemVM> Items { get; set; } = new();

        // Inputs
        [Required, Display(Name = "Full name")]
        public string? FullName { get; set; }

        [Required, EmailAddress, Display(Name = "Email")]
        public string? Email { get; set; }

        [Display(Name = "Address (optional)")]
        public string? Address { get; set; }

        [Display(Name = "Notes (optional)")]
        public string? Notes { get; set; }
    }

    
    public class CartItemVM
    {
        public int ProductId { get; set; }
        public string Title { get; set; } = "";
        public string Category { get; set; } = "";
        public decimal Price { get; set; }
        public int Qty { get; set; }
        public decimal LineTotal => Price * Qty;
    }
}

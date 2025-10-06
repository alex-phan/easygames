using System.ComponentModel.DataAnnotations;
using EasyGames.Models; // Category enum (Book|Game|Toy)

namespace EasyGames.ViewModels
{
    // create form for products // keep it minimal see Notepad p2
    public class AdminProductFormVM
    {
        [Required, StringLength(120)]
        public string Title { get; set; } = string.Empty;

        [Required]
        public Category Category { get; set; } = Category.Book;

        [Range(0.01, 1_000_000)]
        public decimal Price { get; set; }

        [Range(0, 1_000_000)]
        public int StockQty { get; set; }

        [Url, StringLength(400)]
        public string? ImageUrl { get; set; } // optional
    }
}


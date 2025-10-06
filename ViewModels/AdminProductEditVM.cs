using System.ComponentModel.DataAnnotations;
using EasyGames.Models;

namespace EasyGames.ViewModels
{
    // edit form // same fields + Id see Notepad p2.a
    public class AdminProductEditVM
    {
        [Required]
        public int Id { get; set; }

        [Required, StringLength(120)]
        public string Title { get; set; } = string.Empty;

        [Required]
        public Category Category { get; set; } = Category.Book;

        [Range(0.01, 1_000_000)]
        public decimal Price { get; set; }

        [Range(0, 1_000_000)]
        public int StockQty { get; set; }

        [Url, StringLength(400)]
        public string? ImageUrl { get; set; }
    }
}


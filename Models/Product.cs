// product entity for stock 
// product entity for catalog (books/games/toys)
// what: id, title, category, price, stock qty, optional image url
// why: minimal fields to render store cards + admin stock; simple validation to stop bad data early
// note: enum keeps allowed types fixed (no typos); decimal for money to avoid float rounding
using System.ComponentModel.DataAnnotations;

namespace EasyGames.Models
{
    public enum Category { Book, Game, Toy } // what to sell 

    public class Product
    {
        public int Id { get; set; }
        [Required, MaxLength(120)] public string Title { get; set; } = "";
        public Category Category { get; set; }
        [Range(0, 9999)] public decimal Price { get; set; }
        [Range(0, int.MaxValue)] public int StockQty { get; set; }
        public string? ImageUrl { get; set; }
    }
}


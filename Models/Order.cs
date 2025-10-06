// order + items see Notepad n3
using System.ComponentModel.DataAnnotations.Schema;

namespace EasyGames.Models
{
    public enum OrderStatus { Placed, Paid, Shipped } // simple flow see Notepad n3.a

    public class Order
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public User? User { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public OrderStatus Status { get; set; } = OrderStatus.Placed;
        public decimal Total { get; set; }
        public ICollection<OrderItem> Items { get; set; } = new List<OrderItem>();
    }

    public class OrderItem
    {
        public int Id { get; set; }
        public int OrderId { get; set; }
        public Order? Order { get; set; }
        public int ProductId { get; set; }
        public Product? Product { get; set; }
        public int Qty { get; set; }
        public decimal UnitPrice { get; set; }
        [NotMapped] public decimal LineTotal => Qty * UnitPrice; // calc only see Notepad n3.b
    }
}

// order + items see Notepad n3 and doc
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq; // needed for Sum() helpers

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

        // totals stored to keep a historical snapshot at checkout time (price/tax can change later)
        [Column(TypeName = "decimal(18,2)")]
        public decimal Subtotal { get; set; } // sum of line totals at checkout

        [Column(TypeName = "decimal(18,2)")]
        public decimal Tax { get; set; } // simple flat tax number for now

        [Column(TypeName = "decimal(18,2)")]
        public decimal Total { get; set; } // Subtotal + Tax (stored, not re-computed)

        // quick store of profit at order level so reporting is fast
        [Column(TypeName = "decimal(18,2)")]
        public decimal Profit { get; set; } // sum of LineProfit

        public ICollection<OrderItem> Items { get; set; } = new List<OrderItem>();

        // helpers only (not stored) — handy when building the object before saving
        [NotMapped] public decimal CalcSubtotal => Items.Sum(i => i.LineTotal); 
        [NotMapped] public decimal CalcProfit => Items.Sum(i => i.LineProfit);  // line profits rolled up
        [NotMapped] public decimal CalcTotal => CalcSubtotal + Tax;             // basic add for now
    }
}

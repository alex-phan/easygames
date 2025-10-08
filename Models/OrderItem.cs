// order item split to its own file (same model as before) see Notepad n3.b
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EasyGames.Models
{
    public class OrderItem
    {
        public int Id { get; set; }

        public int OrderId { get; set; }
        public Order? Order { get; set; }

        public int ProductId { get; set; }
        public Product? Product { get; set; }

        // qty and prices captured as a snapshot at checkout (unit price can change later in catalog)
        [Range(1, int.MaxValue)]
        public int Qty { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal UnitPrice { get; set; } // sell price at time of purchase

        // store cost at the time of purchase so margin stays correct even if Product.CostPrice changes
        [Column(TypeName = "decimal(18,2)")]
        public decimal CostPriceSnapshot { get; set; } // pulled from Product.CostPrice during checkout

        [NotMapped] public decimal LineTotal => Qty * UnitPrice; 

        // quick helper to show profit per line; stored profit lives at order level for reporting
        [NotMapped] public decimal LineProfit => Qty * (UnitPrice - CostPriceSnapshot); // simple margin helper
    }
}

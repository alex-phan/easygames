// checkout service — place order + decrement stock (transactional) 
using System.ComponentModel.DataAnnotations;
using EasyGames.Data;
using EasyGames.Models;
using Microsoft.EntityFrameworkCore;

namespace EasyGames.Services
{
    public interface ICheckoutService
    {
        // minimal input: user id + product/qty pairs + flat tax number
        Task<Order> PlaceOrderAsync(int userId, IEnumerable<OrderLineInput> lines, decimal tax, CancellationToken ct = default);
    }

    // tiny input record for calling from controller/VM notes for the team 
    public record OrderLineInput([property: Range(1, int.MaxValue)] int ProductId,
                                 [property: Range(1, int.MaxValue)] int Qty);

    public class CheckoutService : ICheckoutService
    {
        private readonly ApplicationDbContext _db;

        public CheckoutService(ApplicationDbContext db) => _db = db;

        public async Task<Order> PlaceOrderAsync(int userId, IEnumerable<OrderLineInput> lines, decimal tax, CancellationToken ct = default)
        {
            // guard rails: no no nulls or empties
            var lineList = lines?.ToList() ?? throw new ArgumentNullException(nameof(lines));
            if (lineList.Count == 0) throw new InvalidOperationException("Cart is empty.");

            // wrap everything in a transaction so stock/rows stay consistent
            await using var tx = await _db.Database.BeginTransactionAsync(ct);

            // pull the products in a single roundtrip
            var ids = lineList.Select(l => l.ProductId).Distinct().ToList();
            var products = await _db.Products
                                    .Where(p => ids.Contains(p.Id))
                                    .ToDictionaryAsync(p => p.Id, ct);

            // basic presence + stock checks (fail fast)
            foreach (var l in lineList)
            {
                if (!products.TryGetValue(l.ProductId, out var p))
                    throw new InvalidOperationException($"Product {l.ProductId} not found.");
                if (p.StockQty < l.Qty)
                    throw new InvalidOperationException($"Insufficient stock for '{p.Title}'. Requested {l.Qty}, have {p.StockQty}.");
            }

            var order = new Order
            {
                UserId = userId,
                CreatedAt = DateTime.UtcNow,
                Status = OrderStatus.Placed,
                Tax = Math.Round(tax, 2)
            };

            // build items with snapshot prices/costs, and decrementt stock
            foreach (var l in lineList)
            {
                var p = products[l.ProductId];

                var item = new OrderItem
                {
                    ProductId = p.Id,
                    Qty = l.Qty,
                    UnitPrice = p.Price,            // snapshot sell price
                    CostPriceSnapshot = p.CostPrice // snapshot cost
                };

                order.Items.Add(item);

                // stock goes down only after validations pass
                p.StockQty -= l.Qty;
            }

            // compute totals + profit; store as snapshots
            order.Subtotal = Math.Round(order.Items.Sum(i => i.LineTotal), 2);
            order.Profit = Math.Round(order.Items.Sum(i => i.LineProfit), 2);
            order.Total = Math.Round(order.Subtotal + order.Tax, 2);

            _db.Orders.Add(order);

            // save all changes (order + items + product stock)
            await _db.SaveChangesAsync(ct);
            await tx.CommitAsync(ct);

            return order;
        }
    }
}

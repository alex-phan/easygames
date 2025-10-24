// Alex Phan:
// A service that handles user tiers based on their lifetime profit.

using EasyGames.Data;
using EasyGames.Models;
using Microsoft.EntityFrameworkCore;

namespace EasyGames.Services
{
    public class TierService(ApplicationDbContext db) : ITierService
    {
        private readonly ApplicationDbContext _db = db;

        // Thresholds:
        // Bronze < $100, Silver < $500, Gold < $2000, else Platinum
        public UserTier GetTier(decimal lifetimeProfit) => lifetimeProfit switch
        {
            < 100m => UserTier.Bronze,
            < 500m => UserTier.Silver,
            < 2000m => UserTier.Gold,
            _ => UserTier.Platinum
        };

        // A task that calculates the user's lifetime profit from all orders and determines their tier.
        public async Task<(UserTier Tier, decimal LifetimeProfit)> GetTierForUserAsync(int userId)
        {
            // LINQ query to sum profits from orders for the specified user.
            var lifetimeProfitQuery =
                from o in _db.Orders.AsNoTracking() // read-only
                where o.UserId == userId
                select (decimal?)o.Profit;

            // Set the lifetime profit or default to 0 if no orders exist.
            var lifetimeProfit = await lifetimeProfitQuery.SumAsync() ?? 0m;

            // Return the tier.
            return (GetTier(lifetimeProfit), lifetimeProfit);
        }
    }
}
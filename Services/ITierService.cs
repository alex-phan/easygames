// Alex Phan:
// An interface to retrieve the user tier. (Alex Phan)

using EasyGames.Models;

namespace EasyGames.Services
{
    public interface ITierService
    {
        UserTier GetTier(decimal lifetimeProfit);
        Task<(UserTier Tier, decimal LifetimeProfit)> GetTierForUserAsync(int userId);
    }
}
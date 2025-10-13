using System.Threading.Tasks;
using EasyGames.Models;

namespace EasyGames.Repositories
{
    public interface IUnitOfWork
    {
        IRepository<Product> Products { get; }
        IRepository<User> Users { get; }

        // ADD THESE TWO LINES
        Task<int> ProductCountAsync();
        Task<int> UserCountAsync();

        Task SaveAsync();
    }
}

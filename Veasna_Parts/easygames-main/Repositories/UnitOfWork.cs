using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using EasyGames.Data;
using EasyGames.Models;

namespace EasyGames.Repositories
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly ApplicationDbContext _db;

        public UnitOfWork(ApplicationDbContext db)
        {
            _db = db;
        }

        // My existing repos:
        public IRepository<Product> Products => new Repository<Product>(_db);
        public IRepository<User> Users => new Repository<User>(_db);

        // ADDed these today 
        public Task<int> ProductCountAsync() => _db.Products.CountAsync();
        public Task<int> UserCountAsync() => _db.Users.CountAsync();

        public Task SaveAsync() => _db.SaveChangesAsync();
    }
}


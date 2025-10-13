using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using EasyGames.Data;

namespace EasyGames.Repositories
{
    public class Repository<T> : IRepository<T> where T : class
    {
        protected readonly ApplicationDbContext _db;
        protected readonly DbSet<T> _set;

        public Repository(ApplicationDbContext db)
        {
            _db = db;
            _set = db.Set<T>();
        }

        public async Task<T?> GetAsync(int id) => await _set.FindAsync(id);

        public async Task<IEnumerable<T>> GetAllAsync(Expression<Func<T, bool>>? filter = null)
            => filter is null ? await _set.ToListAsync()
                              : await _set.Where(filter).ToListAsync();

        public async Task AddAsync(T entity) => await _set.AddAsync(entity);

        public void Update(T entity) => _set.Update(entity);

        public void Remove(T entity) => _set.Remove(entity);

        public Task SaveAsync() => _db.SaveChangesAsync();

        // NEW: implements IRepository<T>.CountAsync
        public Task<int> CountAsync(Expression<Func<T, bool>>? predicate = null)
            => predicate is null ? _set.CountAsync()
                                 : _set.CountAsync(predicate);
    }
}

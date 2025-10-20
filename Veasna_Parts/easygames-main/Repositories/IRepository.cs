// generic repo contract see Notepad r1
using System;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace EasyGames.Repositories
{
    public interface IRepository<T> where T : class
    {
        Task<T?> GetAsync(int id);                      // simple get by id see AI help
        Task<int> CountAsync(Expression<Func<T, bool>>? predicate = null);  // Ai help note
        Task<IEnumerable<T>> GetAllAsync(Expression<Func<T, bool>>? filter = null); // list with filter
        Task AddAsync(T entity);                        // add one
        void Update(T entity);                          // update one
        void Remove(T entity);                          // delete one
        Task SaveAsync();                               // save changes
    }
}


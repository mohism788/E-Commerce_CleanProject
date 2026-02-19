using E_Commerce.Repositrories.Interfaces;

namespace E_Commerce.Repositrories
{
    using global::E_Commerce.Data;
    using Microsoft.EntityFrameworkCore;
    using System.Linq.Expressions;

    namespace E_Commerce.Repositories
    {
        public class GenericRepository<T> : IGenericRepository<T> where T : class
        {
            protected readonly ApplicationDbContext _context;
            protected readonly DbSet<T> _dbSet;

            public GenericRepository(ApplicationDbContext context)
            {
                _context = context;
                _dbSet = context.Set<T>();
            }

            public virtual async Task<T?> GetByIdAsync(int id)
            {
                return await _dbSet.FindAsync(id);
            }

            public virtual async Task<IEnumerable<T>> GetAllAsync()
            {
                return await _dbSet.AsNoTracking().ToListAsync();
            }

            public virtual async Task<T> AddAsync(T entity)
            {
                await _dbSet.AddAsync(entity);
                return entity;
            }

            public virtual Task UpdateAsync(T entity)
            {
                _dbSet.Update(entity);
                return Task.CompletedTask;
            }

            public virtual async Task DeleteAsync(int id)
            {
                var entity = await GetByIdAsync(id);
                if (entity == null)
                {
                    throw new KeyNotFoundException($"Entity with id {id} not found");
                }
                _dbSet.Remove(entity);
            }

            public virtual async Task<bool> ExistsAsync(int id)
            {
                return await _dbSet.FindAsync(id) != null;
            }

            // Optional: Helper methods you might find useful

            public virtual async Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> predicate)
            {
                return await _dbSet.Where(predicate).ToListAsync();
            }

            public virtual async Task<T?> FindFirstAsync(Expression<Func<T, bool>> predicate)
            {
                return await _dbSet.FirstOrDefaultAsync(predicate);
            }

            public virtual async Task<int> CountAsync()
            {
                return await _dbSet.CountAsync();
            }

            public virtual async Task<int> CountAsync(Expression<Func<T, bool>> predicate)
            {
                return await _dbSet.CountAsync(predicate);
            }
        }
    }
}

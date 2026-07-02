using ECommerce.Application.Abstractions.Persistence;
using ECommerce.Domain.Common;
using ECommerce.Infrastructure.Context;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace ECommerce.Infrastructure.Persistence.Repositories
{
    public class GenericRepository<T>(ApplicationDbContext context): IGenericRepository<T> where T : BaseEntity
    {
        private readonly DbSet<T> _dbSet = context.Set<T>();

        public Task AddAsync(T entity)
            => _dbSet.AddAsync(entity).AsTask();

        public Task<bool> AnyAsync(Expression<Func<T, bool>> predicate)
            => _dbSet.AnyAsync(predicate);

        public void Delete(T entity)
            => _dbSet.Remove(entity);

        public Task<T?> FirstOrDefaultAsync(Expression<Func<T, bool>> predicate)
            => _dbSet.FirstOrDefaultAsync(predicate);

        public Task<T?> GetByIdAsync(int id)
            => _dbSet.FindAsync(id).AsTask();

        public void Update(T entity)
            => _dbSet.Update(entity);

        public Task<List<T>> WhereAsync(Expression<Func<T, bool>> predicate)
            => _dbSet.Where(predicate).ToListAsync();
    }
}

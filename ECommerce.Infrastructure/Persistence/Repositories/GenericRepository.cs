using ECommerce.Application.Abstractions.Persistence;
using ECommerce.Domain.Common;
using ECommerce.Infrastructure.Context;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace ECommerce.Infrastructure.Persistence.Repositories
{
    public class GenericRepository<T>(ApplicationDbContext context) : IGenericRepository<T> where T : BaseEntity
    {
        private readonly DbSet<T> _dbSet = context.Set<T>();

        public Task AddAsync(T entity , CancellationToken cancellationToken = default)
            => _dbSet.AddAsync(entity, cancellationToken).AsTask();

        public Task<bool> AnyAsync(Expression<Func<T, bool>> predicate, CancellationToken cancellationToken = default)
            => _dbSet.AnyAsync(predicate, cancellationToken);

        public void Delete(T entity)
            => _dbSet.Remove(entity);

        public Task<T?> FirstOrDefaultAsync(Expression<Func<T, bool>> predicate, CancellationToken cancellationToken = default)
            => _dbSet.FirstOrDefaultAsync(predicate, cancellationToken);

        public Task<T?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
            => _dbSet.FirstOrDefaultAsync(c=> c.Id == id, cancellationToken);   

        public void Update(T entity)
            => _dbSet.Update(entity);

        public Task<List<T>> WhereAsync(Expression<Func<T, bool>> predicate, CancellationToken cancellationToken = default  )
            => _dbSet.Where(predicate).ToListAsync(cancellationToken);

        public Task<int> CountAsync(Expression<Func<T, bool>>? predicate = null, CancellationToken cancellationToken = default)
        {
            IQueryable<T> query = _dbSet.AsNoTracking();

            if (predicate is not null)
                query = query.Where(predicate);

            return query.CountAsync(cancellationToken);
        }

        public async Task<List<T>> GetPagedAsync(Expression<Func<T, bool>>? predicate = null, Func<IQueryable<T>, IOrderedQueryable<T>>? orderBy = null,
                 int skip = 0,
                 int take = 10,
                 CancellationToken cancellationToken = default)
        {
            IQueryable<T> query = _dbSet.AsNoTracking();

            if (predicate is not null)
                query = query.Where(predicate);

            if (orderBy is not null)
                query = orderBy(query);

            return await query
                .Skip(skip)
                .Take(take)
                .ToListAsync(cancellationToken);
        }
    }
}

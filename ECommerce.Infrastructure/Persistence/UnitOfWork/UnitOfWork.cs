using ECommerce.Application.Abstractions.Persistence;
using ECommerce.Infrastructure.Context;

namespace ECommerce.Infrastructure.Persistence.UnitOfWork
{
    public class UnitOfWork(ApplicationDbContext context)
    : IUnitOfWork
    {
        public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
            => context.SaveChangesAsync(cancellationToken);
    }
}

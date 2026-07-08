using ECommerce.Domain.Entities;

namespace ECommerce.Application.Abstractions.Persistence
{
    public interface IProductRepository : IGenericRepository<Product>
    {
        Task<List<Product>> GetPagedWithRelationsAsync(int skip, int take, CancellationToken cancellationToken = default);
        Task<Product?> GetByIdWithRelationsAsync(int id, CancellationToken cancellationToken = default);
    }
}

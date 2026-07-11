using ECommerce.Domain.Entities;

namespace ECommerce.Application.Abstractions.Persistence
{
    public interface IOrderRepository : IGenericRepository<Order>
    {
        Task<List<Order>> GetAllByUserIdAsync(int userId, CancellationToken cancellationToken = default);

        Task<Order?> GetByIdWithDetailsAsync(int orderId, int userId, CancellationToken cancellationToken = default);

        Task<Order?> GetTrackedByIdWithDetailsAsync(int orderId, int userId, CancellationToken cancellationToken = default);

        Task<Order?> GetTrackedByIdAsync(int orderId, int userId, CancellationToken cancellationToken = default);
    }
}

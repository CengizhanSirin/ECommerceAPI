using ECommerce.Domain.Entities;

namespace ECommerce.Application.Abstractions.Persistence
{
    public interface ICartRepository: IGenericRepository<Cart>
    {
        Task<Cart?> GetWithItemsByUserIdAsync( int userId,CancellationToken cancellationToken = default);
    }
}

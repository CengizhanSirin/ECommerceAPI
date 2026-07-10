using ECommerce.Domain.Entities;

namespace ECommerce.Application.Abstractions.Persistence
{
    public interface IRefreshTokenRepository : IGenericRepository<RefreshToken>
    {
        Task<RefreshToken?> GetByTokenAsync(string token, CancellationToken cancellationToken = default);
        Task<RefreshToken?> GetActiveByUserIdAsync(int userId, CancellationToken cancellationToken = default);
    }
}

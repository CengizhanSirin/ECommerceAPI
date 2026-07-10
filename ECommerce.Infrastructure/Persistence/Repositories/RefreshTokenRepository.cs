using ECommerce.Application.Abstractions.Persistence;
using ECommerce.Domain.Entities;
using ECommerce.Infrastructure.Context;
using Microsoft.EntityFrameworkCore;

namespace ECommerce.Infrastructure.Persistence.Repositories
{
    public class RefreshTokenRepository(ApplicationDbContext context) : GenericRepository<RefreshToken>(context), IRefreshTokenRepository
    {
        private readonly ApplicationDbContext _context = context;
        public Task<RefreshToken?> GetActiveByUserIdAsync(int userId, CancellationToken cancellationToken = default)
        {
            return _context.RefreshTokens.FirstOrDefaultAsync(x => x.AppUserId == userId && x.RevokedAt == null && x.ExpiresAt > DateTimeOffset.UtcNow, cancellationToken);
        }

        public Task<RefreshToken?> GetByTokenAsync(string token, CancellationToken cancellationToken = default)
        {
            return _context.RefreshTokens.Include(x => x.AppUser).FirstOrDefaultAsync(x => x.Token == token, cancellationToken);
        }
    }
}

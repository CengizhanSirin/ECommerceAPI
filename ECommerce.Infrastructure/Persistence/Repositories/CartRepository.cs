using ECommerce.Application.Abstractions.Persistence;
using ECommerce.Domain.Entities;
using ECommerce.Infrastructure.Context;
using Microsoft.EntityFrameworkCore;

namespace ECommerce.Infrastructure.Persistence.Repositories
{
    public class CartRepository(ApplicationDbContext context) : GenericRepository<Cart>(context), ICartRepository
    {
        private readonly ApplicationDbContext _context = context;
        public Task<Cart?> GetWithItemsByUserIdAsync(int userId, CancellationToken cancellationToken = default)
        {
            return _context.Carts.Include(x => x.CartItems).ThenInclude(x => x.Product).ThenInclude(x => x.ProductImages).FirstOrDefaultAsync( x => x.AppUserId == userId, cancellationToken);              
        }
    }
}

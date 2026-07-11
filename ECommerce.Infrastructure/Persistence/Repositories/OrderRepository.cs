using ECommerce.Application.Abstractions.Persistence;
using ECommerce.Domain.Entities;
using ECommerce.Infrastructure.Context;
using Microsoft.EntityFrameworkCore;

namespace ECommerce.Infrastructure.Persistence.Repositories
{
    public class OrderRepository(ApplicationDbContext context) : GenericRepository<Order>(context), IOrderRepository
    {
        private readonly ApplicationDbContext _context = context;
        public Task<List<Order>> GetAllByUserIdAsync(int userId, CancellationToken cancellationToken = default)
        {
            return _context.Orders.AsNoTracking().Where(x => x.AppUserId == userId)
                .OrderByDescending(x => x.CreatedDate)
                .ToListAsync(cancellationToken);
        }

        public Task<Order?> GetByIdWithDetailsAsync(int orderId, int userId, CancellationToken cancellationToken = default)
        {
            return _context.Orders.AsNoTracking().Include(x => x.Address).Include(x => x.OrderItems)
                .FirstOrDefaultAsync(x => x.Id == orderId && x.AppUserId == userId, cancellationToken);
        }

        public Task<Order?> GetTrackedByIdAsync(int orderId, int userId, CancellationToken cancellationToken = default)
        {
            return _context.Orders.FirstOrDefaultAsync(x => x.Id == orderId && x.AppUserId == userId, cancellationToken);
        }

        public Task<Order?> GetTrackedByIdWithDetailsAsync(int orderId, int userId, CancellationToken cancellationToken = default)
        {
            return _context.Orders.Include(x => x.OrderItems).FirstOrDefaultAsync(x => x.Id == orderId && x.AppUserId == userId, cancellationToken);
        }
    }
}

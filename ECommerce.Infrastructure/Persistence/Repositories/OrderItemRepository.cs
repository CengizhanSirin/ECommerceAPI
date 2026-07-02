using ECommerce.Application.Abstractions.Persistence;
using ECommerce.Domain.Entities;
using ECommerce.Infrastructure.Context;

namespace ECommerce.Infrastructure.Persistence.Repositories
{
    public class OrderItemRepository(ApplicationDbContext context) : GenericRepository<OrderItem>(context), IOrderItemRepository
    {
    }
}

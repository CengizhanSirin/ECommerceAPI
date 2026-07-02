using ECommerce.Application.Abstractions.Persistence;
using ECommerce.Domain.Entities;
using ECommerce.Infrastructure.Context;

namespace ECommerce.Infrastructure.Persistence.Repositories
{
    public class CartItemRepository(ApplicationDbContext context) : GenericRepository<CartItem>(context), ICartItemRepository   
    {
    }
}

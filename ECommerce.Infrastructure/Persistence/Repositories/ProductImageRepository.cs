using ECommerce.Application.Abstractions.Persistence;
using ECommerce.Domain.Entities;
using ECommerce.Infrastructure.Context;

namespace ECommerce.Infrastructure.Persistence.Repositories
{
    public class ProductImageRepository(ApplicationDbContext context):GenericRepository<ProductImage>(context),IProductImageRepository
    {
    }
}

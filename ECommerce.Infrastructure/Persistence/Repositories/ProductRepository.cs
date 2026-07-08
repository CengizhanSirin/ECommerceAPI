using ECommerce.Application.Abstractions.Persistence;
using ECommerce.Domain.Entities;
using ECommerce.Infrastructure.Context;
using Microsoft.EntityFrameworkCore;

namespace ECommerce.Infrastructure.Persistence.Repositories
{
    public class ProductRepository(ApplicationDbContext context) : GenericRepository<Product>(context), IProductRepository
    {
        private readonly ApplicationDbContext _context = context;

        public Task<Product?> GetByIdWithRelationsAsync(int id, CancellationToken cancellationToken = default)
        {
            return _context.Products
            .AsNoTracking()
            .Include(x => x.Category)
            .Include(x => x.Brand)
            .Include(x => x.ProductImages)
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        }

        public Task<List<Product>> GetPagedWithRelationsAsync(int skip, int take, CancellationToken cancellationToken = default)
        {
            return _context.Products
            .AsNoTracking()
            .Include(x => x.Category)
            .Include(x => x.Brand)
            .Include(x => x.ProductImages)
            .OrderBy(x => x.Name)
            .Skip(skip)
            .Take(take)
            .ToListAsync(cancellationToken);
        }
    }
}

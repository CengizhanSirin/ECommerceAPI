using ECommerce.Application.Abstractions.Persistence;
using ECommerce.Domain.Entities;
using ECommerce.Infrastructure.Context;

namespace ECommerce.Infrastructure.Persistence.Repositories
{
    public class AddressRepository(ApplicationDbContext context) : GenericRepository<Address>(context), IAddressRepository
    {
    }
}

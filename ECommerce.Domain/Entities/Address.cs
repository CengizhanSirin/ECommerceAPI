using ECommerce.Domain.Common;

namespace ECommerce.Domain.Entities
{
    public class Address : AuditableEntity
    {
        public required string Title { get; set; }
        public required string FirstName { get; set; }
        public required string LastName { get; set; }
        public required string Phone { get; set; }
        public required string City { get; set; }
        public required string District { get; set; }
        public required string Neighborhood { get; set; }
        public required string Street { get; set; }
        public string? PostalCode { get; set; }
        public bool IsDefault { get; set; }


        public int AppUserId { get; set; }
        public AppUser AppUser { get; set; } = null!;
    }
}

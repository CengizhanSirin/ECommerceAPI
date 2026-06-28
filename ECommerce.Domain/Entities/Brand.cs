using ECommerce.Domain.Common;

namespace ECommerce.Domain.Entities
{
    public class Brand : AuditableEntity
    {
        public required string Name { get; set; }
        public string? Description { get; set; }
        public string? LogoUrl { get; set; }
        public bool IsActive { get; set; } = true;


        public ICollection<Product> Products { get; set; } = [];
    }
}

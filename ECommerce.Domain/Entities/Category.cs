using ECommerce.Domain.Common;

namespace ECommerce.Domain.Entities
{
    public class Category:AuditableEntity
    {
        public required string Name { get; set; }
        public string? Description { get; set; }
        public string? ImageUrl { get; set; }
        public int  DisplayOrder { get; set; }
        public bool IsActive { get; set; } = true;


        public ICollection<Product> Products { get; set; } = [];
    }
}

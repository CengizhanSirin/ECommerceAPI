using ECommerce.Domain.Common;

namespace ECommerce.Domain.Entities
{
    public class Product : AuditableEntity
    {
        public required string Name { get; set; }
        public string? Description { get; set; }
        public required string SKU { get; set; }
        public decimal Price { get; set; }
        public int Stock { get; set; }
        public bool IsActive { get; set; } = true;

        public int CategoryId { get; set; }
        public Category Category { get; set; } = null!;

        public int BrandId { get; set; }
        public Brand Brand { get; set; } = null!;

        public ICollection<ProductImage> ProductImages { get; set; } = [];
        public ICollection<CartItem> CartItems { get; set; } = [];
        public ICollection<OrderItem> OrderItems { get; set; } = [];
    }
}

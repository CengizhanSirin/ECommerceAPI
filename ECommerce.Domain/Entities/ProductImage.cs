using ECommerce.Domain.Common;

namespace ECommerce.Domain.Entities
{
    public class ProductImage : AuditableEntity
    {
        public required string ImageUrl { get; set; }
        public bool IsMain { get; set; }
        public int DisplayOrder { get; set; }


        public int ProductId { get; set; }
        public Product Product { get; set; } = null!;
    }
}

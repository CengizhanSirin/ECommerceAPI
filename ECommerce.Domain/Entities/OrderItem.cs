using ECommerce.Domain.Common;

namespace ECommerce.Domain.Entities
{
    public class OrderItem : AuditableEntity
    {
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }    
        public required string ProductName { get; set; } 

        public int OrderId { get; set; }
        public Order Order { get; set; } = null!;

        public int ProductId { get; set; }
        public Product Product { get; set; } = null!;
    }
}

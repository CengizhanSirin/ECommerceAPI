using ECommerce.Domain.Common;
using ECommerce.Domain.Enums;

namespace ECommerce.Domain.Entities
{
    public class Order : AuditableEntity
    {
        public required string OrderNumber { get; set; }
        public decimal TotalPrice { get; set; }
        public OrderStatus OrderStatus { get; set; } = OrderStatus.Pending;
        public PaymentStatus PaymentStatus { get; set; } = PaymentStatus.Pending;

        public int AppUserId { get; set; }
        public AppUser AppUser { get; set; } = null!;

        public int AddressId { get; set; }
        public Address Address { get; set; } = null!;

        public ICollection<OrderItem> OrderItems { get; set; } = [];
    }
}

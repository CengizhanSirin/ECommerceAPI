using ECommerce.Domain.Common;

namespace ECommerce.Domain.Entities
{
    public class Cart : AuditableEntity
    {
        public int AppUserId { get; set; }
        public AppUser AppUser { get; set; } = null!;
       

        public ICollection<CartItem> CartItems { get; set; } = [];
    }
}

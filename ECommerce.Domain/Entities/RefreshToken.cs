using ECommerce.Domain.Common;

namespace ECommerce.Domain.Entities
{
    public class RefreshToken : AuditableEntity
    {
        public required string Token { get; set; }

        public DateTimeOffset ExpiresAt { get; set; }

        public DateTimeOffset? RevokedAt { get; set; }

        public int AppUserId { get; set; }

        public AppUser AppUser { get; set; } = null!;
    }
}

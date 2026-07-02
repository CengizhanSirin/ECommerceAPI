using ECommerce.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ECommerce.Infrastructure.Configurations
{
    public class CartConfiguration : IEntityTypeConfiguration<Cart>
    {
        public void Configure(EntityTypeBuilder<Cart> builder)
        {
            builder.ToTable("Carts");

            builder.HasKey(x => x.Id);

            builder.HasIndex(x => x.AppUserId)
                .IsUnique();

            // Relationship

            builder.HasOne(x => x.AppUser)
                .WithOne(x => x.Cart)
                .HasForeignKey<Cart>(x => x.AppUserId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasMany(x => x.CartItems)
                .WithOne(x => x.Cart)
                .HasForeignKey(x => x.CartId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}

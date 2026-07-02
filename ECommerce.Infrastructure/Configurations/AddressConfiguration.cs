using ECommerce.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ECommerce.Infrastructure.Configurations
{
    public class AddressConfiguration : IEntityTypeConfiguration<Address>
    {
        public void Configure(EntityTypeBuilder<Address> builder)
        {
            builder.ToTable("Addresses");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.Title)
                .IsRequired()
                .HasMaxLength(50);

            builder.Property(x => x.FirstName)
                .IsRequired()
                .HasMaxLength(50);

            builder.Property(x => x.LastName)
                .IsRequired()
                .HasMaxLength(50);

            builder.Property(x => x.Phone)
                .IsRequired()
                .HasMaxLength(20);

            builder.Property(x => x.City)
                .IsRequired()
                .HasMaxLength(50);

            builder.Property(x => x.District)
                .IsRequired()
                .HasMaxLength(50);

            builder.Property(x => x.Neighborhood)
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(x => x.Street)
                .IsRequired()
                .HasMaxLength(200);

            builder.Property(x => x.PostalCode)
                .HasMaxLength(10);

            builder.Property(x => x.IsDefault)
                .HasDefaultValue(false);

            

            builder.HasOne(x => x.AppUser)
                .WithMany(x => x.Addresses)
                .HasForeignKey(x => x.AppUserId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}

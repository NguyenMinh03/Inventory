using InventorySystem.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace InventorySystem.Infrastructure.Persistence.Configurations;

public class SupplierConfiguration : IEntityTypeConfiguration<Supplier>
{
    public void Configure(EntityTypeBuilder<Supplier> builder)
    {
        builder.HasKey(s => s.Id);

        builder.Property(s => s.Name)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(s => s.ContactName).HasMaxLength(200);
        builder.Property(s => s.Email).HasMaxLength(256);
        builder.Property(s => s.Phone).HasMaxLength(50);
        builder.Property(s => s.Address).HasMaxLength(500);
    }
}

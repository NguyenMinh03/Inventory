using InventorySystem.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace InventorySystem.Infrastructure.Persistence.Configurations;

public class ProductConfiguration : IEntityTypeConfiguration<Product>
{
    public void Configure(EntityTypeBuilder<Product> builder)
    {
        builder.HasKey(p => p.Id);

        builder.Property(p => p.Sku)
            .IsRequired()
            .HasMaxLength(50);
        builder.HasIndex(p => p.Sku).IsUnique();

        builder.Property(p => p.Name)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(p => p.Description).HasMaxLength(1000);

        builder.Property(p => p.UnitOfMeasure)
            .IsRequired()
            .HasMaxLength(20);

        builder.Property(p => p.UnitPrice).HasColumnType("decimal(18,2)");

        builder.HasOne(p => p.Category)
            .WithMany(c => c.Products)
            .HasForeignKey(p => p.CategoryId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}

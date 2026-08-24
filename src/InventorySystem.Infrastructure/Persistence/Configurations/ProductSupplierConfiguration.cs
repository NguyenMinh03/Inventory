using InventorySystem.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace InventorySystem.Infrastructure.Persistence.Configurations;

public class ProductSupplierConfiguration : IEntityTypeConfiguration<ProductSupplier>
{
    public void Configure(EntityTypeBuilder<ProductSupplier> builder)
    {
        builder.HasKey(ps => ps.Id);

        builder.Property(ps => ps.SupplierSku).HasMaxLength(50);
        builder.Property(ps => ps.UnitCost).HasColumnType("decimal(18,2)");
        builder.Property(ps => ps.LeadTimeDays).IsRequired();

        // A supplier can only be linked to a given product once.
        builder.HasIndex(ps => new { ps.ProductId, ps.SupplierId }).IsUnique();

        builder.HasOne(ps => ps.Product)
            .WithMany(p => p.ProductSuppliers)
            .HasForeignKey(ps => ps.ProductId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(ps => ps.Supplier)
            .WithMany(s => s.ProductSuppliers)
            .HasForeignKey(ps => ps.SupplierId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

using InventorySystem.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace InventorySystem.Infrastructure.Persistence.Configurations;

public class StockMovementConfiguration : IEntityTypeConfiguration<StockMovement>
{
    public void Configure(EntityTypeBuilder<StockMovement> builder)
    {
        builder.HasKey(m => m.Id);

        builder.Property(m => m.Type)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(20);

        builder.Property(m => m.Quantity).IsRequired();
        builder.Property(m => m.Reference).HasMaxLength(100);
        builder.Property(m => m.Notes).HasMaxLength(1000);

        builder.HasOne(m => m.Product)
            .WithMany(p => p.StockMovements)
            .HasForeignKey(m => m.ProductId)
            .OnDelete(DeleteBehavior.Restrict);

        // Both FKs below target Warehouse; SQL Server disallows cascade on more
        // than one path into the same table, so both are Restrict.
        builder.HasOne(m => m.Warehouse)
            .WithMany(w => w.StockMovements)
            .HasForeignKey(m => m.WarehouseId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(m => m.RelatedWarehouse)
            .WithMany()
            .HasForeignKey(m => m.RelatedWarehouseId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}

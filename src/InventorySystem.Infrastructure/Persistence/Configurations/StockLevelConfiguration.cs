using InventorySystem.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace InventorySystem.Infrastructure.Persistence.Configurations;

public class StockLevelConfiguration : IEntityTypeConfiguration<StockLevel>
{
    public void Configure(EntityTypeBuilder<StockLevel> builder)
    {
        // Composite key: at most one stock-level row per product/warehouse pair.
        builder.HasKey(s => new { s.ProductId, s.WarehouseId });

        builder.Property(s => s.QuantityOnHand).IsRequired();
        builder.Property(s => s.LastUpdatedUtc).IsRequired();

        builder.HasOne(s => s.Product)
            .WithMany(p => p.StockLevels)
            .HasForeignKey(s => s.ProductId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(s => s.Warehouse)
            .WithMany(w => w.StockLevels)
            .HasForeignKey(s => s.WarehouseId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

using InventorySystem.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace InventorySystem.Infrastructure.Persistence.Configurations;

public class PurchaseOrderItemConfiguration : IEntityTypeConfiguration<PurchaseOrderItem>
{
    public void Configure(EntityTypeBuilder<PurchaseOrderItem> builder)
    {
        builder.HasKey(i => i.Id);

        builder.Property(i => i.QuantityOrdered).IsRequired();
        builder.Property(i => i.QuantityReceived).IsRequired();
        builder.Property(i => i.UnitCost).HasColumnType("decimal(18,2)");

        builder.HasOne(i => i.Product)
            .WithMany(p => p.PurchaseOrderItems)
            .HasForeignKey(i => i.ProductId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}

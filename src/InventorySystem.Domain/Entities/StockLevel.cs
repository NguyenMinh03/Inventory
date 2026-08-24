namespace InventorySystem.Domain.Entities;

// Composite key (ProductId, WarehouseId): one stock-level row per product/warehouse pair.
public class StockLevel
{
    public int ProductId { get; set; }
    public Product? Product { get; set; }

    public int WarehouseId { get; set; }
    public Warehouse? Warehouse { get; set; }

    public int QuantityOnHand { get; set; }
    public DateTime LastUpdatedUtc { get; set; } = DateTime.UtcNow;
}

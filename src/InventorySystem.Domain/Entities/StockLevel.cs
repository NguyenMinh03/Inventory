namespace InventorySystem.Domain.Entities;

public class StockLevel
{
    public int Id { get; set; }

    public int ProductId { get; set; }
    public Product? Product { get; set; }

    public int WarehouseId { get; set; }
    public Warehouse? Warehouse { get; set; }

    public int QuantityOnHand { get; set; }
    public DateTime LastUpdatedUtc { get; set; } = DateTime.UtcNow;
}

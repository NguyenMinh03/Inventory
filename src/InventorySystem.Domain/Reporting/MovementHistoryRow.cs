namespace InventorySystem.Domain.Reporting;

// A query projection, not an entity: one flattened row per StockMovement,
// with product/warehouse names already joined in.
public class MovementHistoryRow
{
    public int Id { get; set; }
    public int ProductId { get; set; }
    public string ProductSku { get; set; } = string.Empty;
    public string ProductName { get; set; } = string.Empty;
    public int WarehouseId { get; set; }
    public string WarehouseName { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public DateTime OccurredUtc { get; set; }
    public string? Reference { get; set; }
}

using InventorySystem.Domain.Enums;

namespace InventorySystem.Domain.Entities;

public class StockMovement
{
    public int Id { get; set; }

    public int ProductId { get; set; }
    public Product? Product { get; set; }

    public int WarehouseId { get; set; }
    public Warehouse? Warehouse { get; set; }

    // Destination warehouse; only set when Type is Transfer.
    public int? RelatedWarehouseId { get; set; }
    public Warehouse? RelatedWarehouse { get; set; }

    public MovementType Type { get; set; }
    public int Quantity { get; set; }
    public DateTime OccurredUtc { get; set; } = DateTime.UtcNow;
    public string? Reference { get; set; }
    public string? Notes { get; set; }
}

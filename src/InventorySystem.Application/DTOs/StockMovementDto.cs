using InventorySystem.Domain.Enums;

namespace InventorySystem.Application.DTOs;

public class StockMovementDto
{
    public int Id { get; set; }
    public int ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public int WarehouseId { get; set; }
    public string WarehouseName { get; set; } = string.Empty;
    public int? RelatedWarehouseId { get; set; }
    public string? RelatedWarehouseName { get; set; }
    public MovementType Type { get; set; }
    public int Quantity { get; set; }
    public DateTime OccurredUtc { get; set; }
    public string? Reference { get; set; }
    public string? Notes { get; set; }
}

// For single-warehouse movements only (In / Out / Adjustment). Transfers
// between warehouses go through CreateStockTransferDto / TransferAsync instead.
public class CreateStockMovementDto
{
    public int ProductId { get; set; }
    public int WarehouseId { get; set; }
    public MovementType Type { get; set; }
    public int Quantity { get; set; }
    public string? Reference { get; set; }
    public string? Notes { get; set; }
}

public class CreateStockTransferDto
{
    public int ProductId { get; set; }
    public int SourceWarehouseId { get; set; }
    public int DestinationWarehouseId { get; set; }
    public int Quantity { get; set; }
    public string? Reference { get; set; }
    public string? Notes { get; set; }
}

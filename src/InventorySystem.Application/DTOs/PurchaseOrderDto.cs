using InventorySystem.Domain.Enums;

namespace InventorySystem.Application.DTOs;

public class PurchaseOrderDto
{
    public int Id { get; set; }
    public int SupplierId { get; set; }
    public string SupplierName { get; set; } = string.Empty;
    public DateTime OrderDateUtc { get; set; }
    public DateTime? ExpectedDeliveryDateUtc { get; set; }
    public PurchaseOrderStatus Status { get; set; }
    public string? Notes { get; set; }
    public List<PurchaseOrderItemDto> Items { get; set; } = [];
}

public class PurchaseOrderItemDto
{
    public int Id { get; set; }
    public int ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public int QuantityOrdered { get; set; }
    public int QuantityReceived { get; set; }
    public decimal UnitCost { get; set; }
}

public class CreatePurchaseOrderDto
{
    public int SupplierId { get; set; }
    public DateTime? ExpectedDeliveryDateUtc { get; set; }
    public string? Notes { get; set; }
    public List<CreatePurchaseOrderItemDto> Items { get; set; } = [];
}

public class CreatePurchaseOrderItemDto
{
    public int ProductId { get; set; }
    public int QuantityOrdered { get; set; }
    public decimal UnitCost { get; set; }
}

public class ReceivePurchaseOrderDto
{
    public int WarehouseId { get; set; }
    public List<ReceivePurchaseOrderItemDto> Items { get; set; } = [];
}

public class ReceivePurchaseOrderItemDto
{
    public int PurchaseOrderItemId { get; set; }
    public int QuantityReceived { get; set; }
}

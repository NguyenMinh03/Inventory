namespace InventorySystem.Application.DTOs;

public class StockValuationDto
{
    public int GroupId { get; set; }
    public string GroupName { get; set; } = string.Empty;
    public int TotalQuantityOnHand { get; set; }
    public decimal TotalValue { get; set; }
}

public class MovementHistoryItemDto
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

public class PagedResult<T>
{
    public IReadOnlyList<T> Items { get; set; } = [];
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalCount { get; set; }
    public int TotalPages => PageSize == 0 ? 0 : (int)Math.Ceiling(TotalCount / (double)PageSize);
}

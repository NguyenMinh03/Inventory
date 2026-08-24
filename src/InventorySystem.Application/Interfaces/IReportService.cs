using InventorySystem.Application.DTOs;

namespace InventorySystem.Application.Interfaces;

public interface IReportService
{
    Task<IReadOnlyList<ProductDto>> GetLowStockAsync();
    Task<IReadOnlyList<StockValuationDto>> GetStockValuationByWarehouseAsync();
    Task<IReadOnlyList<StockValuationDto>> GetStockValuationByCategoryAsync();

    Task<PagedResult<MovementHistoryItemDto>> GetMovementHistoryAsync(
        DateTime? from, DateTime? to, int? productId, int page, int pageSize);
}

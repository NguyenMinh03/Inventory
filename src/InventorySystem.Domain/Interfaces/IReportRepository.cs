using InventorySystem.Domain.Reporting;

namespace InventorySystem.Domain.Interfaces;

public interface IReportRepository
{
    Task<IReadOnlyList<StockValuationRow>> GetStockValuationByWarehouseAsync();
    Task<IReadOnlyList<StockValuationRow>> GetStockValuationByCategoryAsync();

    Task<(IReadOnlyList<MovementHistoryRow> Items, int TotalCount)> GetMovementHistoryAsync(
        DateTime? from, DateTime? to, int? productId, int page, int pageSize);
}

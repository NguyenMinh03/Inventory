using InventorySystem.Application.DTOs;
using InventorySystem.Application.Interfaces;
using InventorySystem.Domain.Interfaces;
using InventorySystem.Domain.Reporting;

namespace InventorySystem.Application.Services;

public class ReportService : IReportService
{
    private readonly IStockService _stockService;
    private readonly IReportRepository _reportRepository;

    public ReportService(IStockService stockService, IReportRepository reportRepository)
    {
        _stockService = stockService;
        _reportRepository = reportRepository;
    }

    // Low-stock is already computed by StockService; no need to duplicate it here.
    public Task<IReadOnlyList<ProductDto>> GetLowStockAsync() => _stockService.GetLowStockProductsAsync();

    public async Task<IReadOnlyList<StockValuationDto>> GetStockValuationByWarehouseAsync()
    {
        var rows = await _reportRepository.GetStockValuationByWarehouseAsync();
        return rows.Select(MapValuation).ToList();
    }

    public async Task<IReadOnlyList<StockValuationDto>> GetStockValuationByCategoryAsync()
    {
        var rows = await _reportRepository.GetStockValuationByCategoryAsync();
        return rows.Select(MapValuation).ToList();
    }

    public async Task<PagedResult<MovementHistoryItemDto>> GetMovementHistoryAsync(
        DateTime? from, DateTime? to, int? productId, int page, int pageSize)
    {
        page = page < 1 ? 1 : page;
        pageSize = pageSize is < 1 or > 200 ? 50 : pageSize;

        var (rows, totalCount) = await _reportRepository.GetMovementHistoryAsync(from, to, productId, page, pageSize);

        return new PagedResult<MovementHistoryItemDto>
        {
            Items = rows.Select(r => new MovementHistoryItemDto
            {
                Id = r.Id,
                ProductId = r.ProductId,
                ProductSku = r.ProductSku,
                ProductName = r.ProductName,
                WarehouseId = r.WarehouseId,
                WarehouseName = r.WarehouseName,
                Type = r.Type,
                Quantity = r.Quantity,
                OccurredUtc = r.OccurredUtc,
                Reference = r.Reference,
            }).ToList(),
            Page = page,
            PageSize = pageSize,
            TotalCount = totalCount,
        };
    }

    private static StockValuationDto MapValuation(StockValuationRow row) => new()
    {
        GroupId = row.GroupId,
        GroupName = row.GroupName,
        TotalQuantityOnHand = row.TotalQuantityOnHand,
        TotalValue = row.TotalValue,
    };
}

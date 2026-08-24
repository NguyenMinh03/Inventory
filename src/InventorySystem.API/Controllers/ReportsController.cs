using InventorySystem.Application.DTOs;
using InventorySystem.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace InventorySystem.API.Controllers;

[ApiController]
[Route("api/reports")]
public class ReportsController : ControllerBase
{
    private readonly IReportService _reportService;

    public ReportsController(IReportService reportService)
    {
        _reportService = reportService;
    }

    [HttpGet("low-stock")]
    public async Task<ActionResult<IReadOnlyList<ProductDto>>> GetLowStock() =>
        Ok(await _reportService.GetLowStockAsync());

    // by=warehouse (default) or by=category
    [HttpGet("stock-valuation")]
    public async Task<ActionResult<IReadOnlyList<StockValuationDto>>> GetStockValuation([FromQuery] string by = "warehouse")
    {
        if (string.Equals(by, "category", StringComparison.OrdinalIgnoreCase))
            return Ok(await _reportService.GetStockValuationByCategoryAsync());

        return Ok(await _reportService.GetStockValuationByWarehouseAsync());
    }

    [HttpGet("movement-history")]
    public async Task<ActionResult<PagedResult<MovementHistoryItemDto>>> GetMovementHistory(
        [FromQuery] DateTime? from,
        [FromQuery] DateTime? to,
        [FromQuery] int? productId,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 50)
    {
        return Ok(await _reportService.GetMovementHistoryAsync(from, to, productId, page, pageSize));
    }
}

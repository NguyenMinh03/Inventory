using InventorySystem.Application.DTOs;

namespace InventorySystem.Application.Interfaces;

public interface IStockService
{
    Task<IReadOnlyList<StockLevelDto>> GetStockLevelsAsync();
    Task<IReadOnlyList<StockLevelDto>> GetStockLevelsByProductAsync(int productId);
    Task<IReadOnlyList<ProductDto>> GetLowStockProductsAsync();
    Task<StockMovementDto> RecordMovementAsync(CreateStockMovementDto dto);
}

using InventorySystem.Application.DTOs;

namespace InventorySystem.Application.Interfaces;

public interface IStockService
{
    Task<IReadOnlyList<StockLevelDto>> GetStockLevelsAsync();
    Task<IReadOnlyList<StockLevelDto>> GetStockLevelsByProductAsync(int productId);
    Task<IReadOnlyList<ProductDto>> GetLowStockProductsAsync();
    Task<StockMovementDto> RecordMovementAsync(CreateStockMovementDto dto);

    // Modeled as one Out movement at the source warehouse plus one In movement
    // at the destination, both committed by a single SaveChangesAsync call so
    // the transfer is all-or-nothing.
    Task<IReadOnlyList<StockMovementDto>> TransferAsync(CreateStockTransferDto dto);
}

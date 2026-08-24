using InventorySystem.Domain.Entities;

namespace InventorySystem.Domain.Interfaces;

// StockLevel uses a composite (ProductId, WarehouseId) key, so it can't
// be looked up through the generic IRepository<T>.GetByIdAsync(int id).
public interface IStockLevelRepository
{
    Task<StockLevel?> GetByIdAsync(int productId, int warehouseId);
    Task<IReadOnlyList<StockLevel>> GetAllAsync();
    Task<IReadOnlyList<StockLevel>> GetByProductIdAsync(int productId);
    Task AddAsync(StockLevel entity);
    void Update(StockLevel entity);
    void Remove(StockLevel entity);
}

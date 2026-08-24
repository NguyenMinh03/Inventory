using InventorySystem.Domain.Entities;
using InventorySystem.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace InventorySystem.Infrastructure.Persistence.Repositories;

public class StockLevelRepository : IStockLevelRepository
{
    private readonly AppDbContext _context;

    public StockLevelRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<StockLevel?> GetByIdAsync(int productId, int warehouseId) =>
        await _context.StockLevels.FindAsync(productId, warehouseId);

    public async Task<IReadOnlyList<StockLevel>> GetAllAsync() =>
        await _context.StockLevels.ToListAsync();

    public async Task<IReadOnlyList<StockLevel>> GetByProductIdAsync(int productId) =>
        await _context.StockLevels.Where(s => s.ProductId == productId).ToListAsync();

    public async Task AddAsync(StockLevel entity) => await _context.StockLevels.AddAsync(entity);

    public void Update(StockLevel entity) => _context.StockLevels.Update(entity);

    public void Remove(StockLevel entity) => _context.StockLevels.Remove(entity);
}

using InventorySystem.Domain.Entities;
using InventorySystem.Domain.Interfaces;
using InventorySystem.Infrastructure.Persistence.Repositories;

namespace InventorySystem.Infrastructure.Persistence;

public class UnitOfWork : IUnitOfWork, IDisposable
{
    private readonly AppDbContext _context;

    private IRepository<Product>? _products;
    private IRepository<Category>? _categories;
    private IRepository<Warehouse>? _warehouses;
    private IRepository<Supplier>? _suppliers;
    private IStockLevelRepository? _stockLevels;
    private IRepository<StockMovement>? _stockMovements;
    private IRepository<PurchaseOrder>? _purchaseOrders;
    private IRepository<PurchaseOrderItem>? _purchaseOrderItems;
    private IRepository<ProductSupplier>? _productSuppliers;
    private IUserRepository? _users;

    public UnitOfWork(AppDbContext context)
    {
        _context = context;
    }

    public IRepository<Product> Products => _products ??= new Repository<Product>(_context);
    public IRepository<Category> Categories => _categories ??= new Repository<Category>(_context);
    public IRepository<Warehouse> Warehouses => _warehouses ??= new Repository<Warehouse>(_context);
    public IRepository<Supplier> Suppliers => _suppliers ??= new Repository<Supplier>(_context);
    public IStockLevelRepository StockLevels => _stockLevels ??= new StockLevelRepository(_context);
    public IRepository<StockMovement> StockMovements => _stockMovements ??= new Repository<StockMovement>(_context);
    public IRepository<PurchaseOrder> PurchaseOrders => _purchaseOrders ??= new Repository<PurchaseOrder>(_context);
    public IRepository<PurchaseOrderItem> PurchaseOrderItems => _purchaseOrderItems ??= new Repository<PurchaseOrderItem>(_context);
    public IRepository<ProductSupplier> ProductSuppliers => _productSuppliers ??= new Repository<ProductSupplier>(_context);
    public IUserRepository Users => _users ??= new UserRepository(_context);

    public Task<int> SaveChangesAsync() => _context.SaveChangesAsync();

    public void Dispose() => _context.Dispose();
}

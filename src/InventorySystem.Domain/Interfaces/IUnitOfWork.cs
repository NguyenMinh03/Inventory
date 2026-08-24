using InventorySystem.Domain.Entities;

namespace InventorySystem.Domain.Interfaces;

public interface IUnitOfWork
{
    IRepository<Product> Products { get; }
    IRepository<Category> Categories { get; }
    IRepository<Warehouse> Warehouses { get; }
    IRepository<Supplier> Suppliers { get; }
    IRepository<StockLevel> StockLevels { get; }
    IRepository<StockMovement> StockMovements { get; }
    IRepository<PurchaseOrder> PurchaseOrders { get; }
    IRepository<PurchaseOrderItem> PurchaseOrderItems { get; }
    IRepository<ProductSupplier> ProductSuppliers { get; }

    Task<int> SaveChangesAsync();
}

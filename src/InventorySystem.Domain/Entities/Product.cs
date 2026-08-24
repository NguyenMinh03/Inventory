using InventorySystem.Domain.Exceptions;

namespace InventorySystem.Domain.Entities;

public class Product
{
    public int Id { get; set; }
    public string Sku { get; private set; } = string.Empty;
    public string Name { get; private set; } = string.Empty;
    public string? Description { get; set; }
    public string UnitOfMeasure { get; set; } = string.Empty;
    public decimal UnitPrice { get; set; }
    public int ReorderLevel { get; private set; }
    public bool IsActive { get; set; } = true;

    public int CategoryId { get; set; }
    public Category? Category { get; set; }

    public ICollection<StockLevel> StockLevels { get; set; } = new List<StockLevel>();
    public ICollection<StockMovement> StockMovements { get; set; } = new List<StockMovement>();
    public ICollection<ProductSupplier> ProductSuppliers { get; set; } = new List<ProductSupplier>();
    public ICollection<PurchaseOrderItem> PurchaseOrderItems { get; set; } = new List<PurchaseOrderItem>();

    private Product()
    {
        // Reserved for ORM materialization.
    }

    public Product(string sku, string name, string unitOfMeasure, decimal unitPrice, int reorderLevel, int categoryId)
    {
        if (string.IsNullOrWhiteSpace(sku))
            throw new DomainException("SKU is required.");
        if (string.IsNullOrWhiteSpace(name))
            throw new DomainException("Product name is required.");
        if (reorderLevel < 0)
            throw new DomainException("Reorder level cannot be negative.");

        Sku = sku;
        Name = name;
        UnitOfMeasure = unitOfMeasure;
        UnitPrice = unitPrice;
        ReorderLevel = reorderLevel;
        CategoryId = categoryId;
    }

    public void Rename(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new DomainException("Product name is required.");

        Name = name;
    }

    public void UpdateReorderLevel(int reorderLevel)
    {
        if (reorderLevel < 0)
            throw new DomainException("Reorder level cannot be negative.");

        ReorderLevel = reorderLevel;
    }

    public bool IsBelowReorderLevel(int quantityOnHand) => quantityOnHand < ReorderLevel;
}

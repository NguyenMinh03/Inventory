namespace InventorySystem.Domain.Entities;

// Many-to-many link between Product and Supplier, carrying supplier-specific
// terms for that product (their SKU, cost, and lead time).
public class ProductSupplier
{
    public int Id { get; set; }

    public int ProductId { get; set; }
    public Product? Product { get; set; }

    public int SupplierId { get; set; }
    public Supplier? Supplier { get; set; }

    public string? SupplierSku { get; set; }
    public decimal UnitCost { get; set; }
    public int LeadTimeDays { get; set; }
}

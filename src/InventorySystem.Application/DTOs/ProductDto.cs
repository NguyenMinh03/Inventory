namespace InventorySystem.Application.DTOs;

public class ProductDto
{
    public int Id { get; set; }
    public string Sku { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string UnitOfMeasure { get; set; } = string.Empty;
    public decimal UnitPrice { get; set; }
    public int ReorderLevel { get; set; }
    public bool IsActive { get; set; }
    public int CategoryId { get; set; }
    public string? CategoryName { get; set; }
}

public class CreateProductDto
{
    public string Sku { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string UnitOfMeasure { get; set; } = string.Empty;
    public decimal UnitPrice { get; set; }
    public int ReorderLevel { get; set; }
    public int CategoryId { get; set; }
}

public class UpdateProductDto
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string UnitOfMeasure { get; set; } = string.Empty;
    public decimal UnitPrice { get; set; }
    public int ReorderLevel { get; set; }
    public int CategoryId { get; set; }
    public bool IsActive { get; set; }
}

public class ProductQueryDto
{
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;

    // Matches against Sku or Name, case-insensitive.
    public string? Search { get; set; }

    // One of: name, name_desc, sku, sku_desc, price, price_desc,
    // reorderlevel, reorderlevel_desc. Defaults to name ascending.
    public string? SortBy { get; set; }
}

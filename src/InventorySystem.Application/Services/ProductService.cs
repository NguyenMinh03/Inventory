using AutoMapper;
using InventorySystem.Application.DTOs;
using InventorySystem.Application.Interfaces;
using InventorySystem.Domain.Entities;
using InventorySystem.Domain.Interfaces;

namespace InventorySystem.Application.Services;

public class ProductService : IProductService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public ProductService(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    // Filters/sorts/pages in memory after loading the full product table. Fine at
    // this dataset's scale; a larger catalog would push this down to the database
    // via a dedicated paged-query repository method (as IReportRepository does
    // for reporting) instead of materializing everything first.
    public async Task<PagedResult<ProductDto>> GetAllAsync(ProductQueryDto query)
    {
        var products = await _unitOfWork.Products.GetAllAsync("Category");

        IEnumerable<Product> filtered = products;
        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var search = query.Search.Trim();
            filtered = filtered.Where(p =>
                p.Name.Contains(search, StringComparison.OrdinalIgnoreCase) ||
                p.Sku.Contains(search, StringComparison.OrdinalIgnoreCase));
        }

        filtered = query.SortBy?.Trim().ToLowerInvariant() switch
        {
            "name_desc" => filtered.OrderByDescending(p => p.Name),
            "sku" => filtered.OrderBy(p => p.Sku),
            "sku_desc" => filtered.OrderByDescending(p => p.Sku),
            "price" => filtered.OrderBy(p => p.UnitPrice),
            "price_desc" => filtered.OrderByDescending(p => p.UnitPrice),
            "reorderlevel" => filtered.OrderBy(p => p.ReorderLevel),
            "reorderlevel_desc" => filtered.OrderByDescending(p => p.ReorderLevel),
            _ => filtered.OrderBy(p => p.Name),
        };

        var materialized = filtered.ToList();
        var page = query.Page < 1 ? 1 : query.Page;
        var pageSize = query.PageSize is < 1 or > 200 ? 20 : query.PageSize;

        var pageItems = materialized.Skip((page - 1) * pageSize).Take(pageSize);

        return new PagedResult<ProductDto>
        {
            Items = _mapper.Map<List<ProductDto>>(pageItems),
            Page = page,
            PageSize = pageSize,
            TotalCount = materialized.Count,
        };
    }

    public async Task<ProductDto?> GetByIdAsync(int id)
    {
        var product = await _unitOfWork.Products.GetByIdAsync(id, "Category");
        return product is null ? null : _mapper.Map<ProductDto>(product);
    }

    public async Task<ProductDto> CreateAsync(CreateProductDto dto)
    {
        if (await _unitOfWork.Categories.GetByIdAsync(dto.CategoryId) is null)
            throw new KeyNotFoundException($"Category {dto.CategoryId} was not found.");

        var product = new Product(dto.Sku, dto.Name, dto.UnitOfMeasure, dto.UnitPrice, dto.ReorderLevel, dto.CategoryId)
        {
            Description = dto.Description,
        };

        await _unitOfWork.Products.AddAsync(product);
        await _unitOfWork.SaveChangesAsync();

        return _mapper.Map<ProductDto>(product);
    }

    public async Task UpdateAsync(int id, UpdateProductDto dto)
    {
        var product = await _unitOfWork.Products.GetByIdAsync(id)
            ?? throw new KeyNotFoundException($"Product {id} was not found.");

        if (await _unitOfWork.Categories.GetByIdAsync(dto.CategoryId) is null)
            throw new KeyNotFoundException($"Category {dto.CategoryId} was not found.");

        product.Rename(dto.Name);
        product.UpdateReorderLevel(dto.ReorderLevel);
        product.Description = dto.Description;
        product.UnitOfMeasure = dto.UnitOfMeasure;
        product.UnitPrice = dto.UnitPrice;
        product.CategoryId = dto.CategoryId;
        product.IsActive = dto.IsActive;

        _unitOfWork.Products.Update(product);
        await _unitOfWork.SaveChangesAsync();
    }

    public async Task DeleteAsync(int id)
    {
        var product = await _unitOfWork.Products.GetByIdAsync(id)
            ?? throw new KeyNotFoundException($"Product {id} was not found.");

        _unitOfWork.Products.Remove(product);
        await _unitOfWork.SaveChangesAsync();
    }
}

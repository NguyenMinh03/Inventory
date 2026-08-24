using AutoMapper;
using InventorySystem.Application.DTOs;
using InventorySystem.Domain.Entities;

namespace InventorySystem.Application.Mappings;

// Entity -> DTO only: Create/Update DTOs are turned into entities by the
// services themselves, through domain constructors and behavior methods
// rather than blind property copying.
public class MappingProfile : Profile
{
    public MappingProfile()
    {
        CreateMap<Category, CategoryDto>()
            .ForMember(d => d.ParentCategoryName, o => o.MapFrom(s => s.ParentCategory != null ? s.ParentCategory.Name : null));

        CreateMap<Warehouse, WarehouseDto>();

        CreateMap<Supplier, SupplierDto>();

        CreateMap<Product, ProductDto>()
            .ForMember(d => d.CategoryName, o => o.MapFrom(s => s.Category != null ? s.Category.Name : null));

        CreateMap<StockLevel, StockLevelDto>()
            .ForMember(d => d.ProductSku, o => o.MapFrom(s => s.Product != null ? s.Product.Sku : string.Empty))
            .ForMember(d => d.ProductName, o => o.MapFrom(s => s.Product != null ? s.Product.Name : string.Empty))
            .ForMember(d => d.WarehouseName, o => o.MapFrom(s => s.Warehouse != null ? s.Warehouse.Name : string.Empty));

        CreateMap<StockMovement, StockMovementDto>()
            .ForMember(d => d.ProductName, o => o.MapFrom(s => s.Product != null ? s.Product.Name : string.Empty))
            .ForMember(d => d.WarehouseName, o => o.MapFrom(s => s.Warehouse != null ? s.Warehouse.Name : string.Empty))
            .ForMember(d => d.RelatedWarehouseName, o => o.MapFrom(s => s.RelatedWarehouse != null ? s.RelatedWarehouse.Name : null));

        CreateMap<PurchaseOrder, PurchaseOrderDto>()
            .ForMember(d => d.SupplierName, o => o.MapFrom(s => s.Supplier != null ? s.Supplier.Name : string.Empty));

        CreateMap<PurchaseOrderItem, PurchaseOrderItemDto>()
            .ForMember(d => d.ProductName, o => o.MapFrom(s => s.Product != null ? s.Product.Name : string.Empty));
    }
}

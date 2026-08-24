using AutoMapper;
using InventorySystem.Application.DTOs;
using InventorySystem.Application.Interfaces;
using InventorySystem.Domain.Entities;
using InventorySystem.Domain.Enums;
using InventorySystem.Domain.Exceptions;
using InventorySystem.Domain.Interfaces;

namespace InventorySystem.Application.Services;

public class StockService : IStockService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public StockService(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<IReadOnlyList<StockLevelDto>> GetStockLevelsAsync()
    {
        var stockLevels = await _unitOfWork.StockLevels.GetAllAsync();
        return _mapper.Map<List<StockLevelDto>>(stockLevels);
    }

    public async Task<IReadOnlyList<StockLevelDto>> GetStockLevelsByProductAsync(int productId)
    {
        var stockLevels = await _unitOfWork.StockLevels.GetByProductIdAsync(productId);
        return _mapper.Map<List<StockLevelDto>>(stockLevels);
    }

    public async Task<IReadOnlyList<ProductDto>> GetLowStockProductsAsync()
    {
        var products = await _unitOfWork.Products.GetAllAsync("Category");
        var stockLevels = await _unitOfWork.StockLevels.GetAllAsync();

        var onHandByProduct = stockLevels
            .GroupBy(s => s.ProductId)
            .ToDictionary(g => g.Key, g => g.Sum(s => s.QuantityOnHand));

        var lowStock = products.Where(p => p.IsBelowReorderLevel(onHandByProduct.GetValueOrDefault(p.Id)));

        return _mapper.Map<List<ProductDto>>(lowStock);
    }

    public async Task<StockMovementDto> RecordMovementAsync(CreateStockMovementDto dto)
    {
        if (await _unitOfWork.Products.GetByIdAsync(dto.ProductId) is null)
            throw new KeyNotFoundException($"Product {dto.ProductId} was not found.");
        if (await _unitOfWork.Warehouses.GetByIdAsync(dto.WarehouseId) is null)
            throw new KeyNotFoundException($"Warehouse {dto.WarehouseId} was not found.");
        if (dto.Quantity <= 0)
            throw new DomainException("Movement quantity must be positive.");

        switch (dto.Type)
        {
            case MovementType.In:
            case MovementType.Adjustment:
                await AdjustStockAsync(dto.ProductId, dto.WarehouseId, dto.Quantity);
                break;

            case MovementType.Out:
                await AdjustStockAsync(dto.ProductId, dto.WarehouseId, -dto.Quantity);
                break;

            case MovementType.Transfer:
                if (dto.RelatedWarehouseId is null)
                    throw new DomainException("Transfer movements require a destination warehouse.");
                if (dto.RelatedWarehouseId == dto.WarehouseId)
                    throw new DomainException("Transfer source and destination warehouses must differ.");
                if (await _unitOfWork.Warehouses.GetByIdAsync(dto.RelatedWarehouseId.Value) is null)
                    throw new KeyNotFoundException($"Warehouse {dto.RelatedWarehouseId} was not found.");

                await AdjustStockAsync(dto.ProductId, dto.WarehouseId, -dto.Quantity);
                await AdjustStockAsync(dto.ProductId, dto.RelatedWarehouseId.Value, dto.Quantity);
                break;
        }

        var movement = new StockMovement
        {
            ProductId = dto.ProductId,
            WarehouseId = dto.WarehouseId,
            RelatedWarehouseId = dto.RelatedWarehouseId,
            Type = dto.Type,
            Quantity = dto.Quantity,
            Reference = dto.Reference,
            Notes = dto.Notes,
            OccurredUtc = DateTime.UtcNow,
        };

        await _unitOfWork.StockMovements.AddAsync(movement);
        await _unitOfWork.SaveChangesAsync();

        return _mapper.Map<StockMovementDto>(movement);
    }

    // delta is signed: positive adds stock, negative removes it. Throws
    // InsufficientStockException rather than letting a warehouse go negative.
    private async Task AdjustStockAsync(int productId, int warehouseId, int delta)
    {
        var stockLevel = await _unitOfWork.StockLevels.GetByIdAsync(productId, warehouseId);
        if (stockLevel is null)
        {
            if (delta < 0)
                throw new InsufficientStockException(
                    $"Cannot remove {-delta} units of product {productId} from warehouse {warehouseId}; none on hand.");

            stockLevel = new StockLevel { ProductId = productId, WarehouseId = warehouseId, QuantityOnHand = 0 };
            await _unitOfWork.StockLevels.AddAsync(stockLevel);
        }

        var newQuantity = stockLevel.QuantityOnHand + delta;
        if (newQuantity < 0)
            throw new InsufficientStockException(
                $"Cannot remove {-delta} units of product {productId} from warehouse {warehouseId}; only {stockLevel.QuantityOnHand} on hand.");

        stockLevel.QuantityOnHand = newQuantity;
        stockLevel.LastUpdatedUtc = DateTime.UtcNow;
    }
}

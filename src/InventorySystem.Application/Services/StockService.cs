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
        var product = await _unitOfWork.Products.GetByIdAsync(dto.ProductId)
            ?? throw new KeyNotFoundException($"Product {dto.ProductId} was not found.");
        var warehouse = await _unitOfWork.Warehouses.GetByIdAsync(dto.WarehouseId)
            ?? throw new KeyNotFoundException($"Warehouse {dto.WarehouseId} was not found.");
        if (dto.Quantity <= 0)
            throw new DomainException("Movement quantity must be positive.");

        var delta = dto.Type switch
        {
            MovementType.In or MovementType.Adjustment => dto.Quantity,
            MovementType.Out => -dto.Quantity,
            _ => throw new DomainException(
                $"Movement type {dto.Type} is not accepted here; use POST /api/stock/transfers for transfers."),
        };

        // Validating and staging the StockLevel change before the movement row is
        // added means a failure here (e.g. InsufficientStockException) leaves the
        // change tracker untouched - nothing gets written since SaveChangesAsync
        // is never reached.
        await AdjustStockAsync(dto.ProductId, dto.WarehouseId, delta);

        var movement = new StockMovement
        {
            ProductId = dto.ProductId,
            Product = product,
            WarehouseId = dto.WarehouseId,
            Warehouse = warehouse,
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

    public async Task<IReadOnlyList<StockMovementDto>> TransferAsync(CreateStockTransferDto dto)
    {
        var product = await _unitOfWork.Products.GetByIdAsync(dto.ProductId)
            ?? throw new KeyNotFoundException($"Product {dto.ProductId} was not found.");
        var sourceWarehouse = await _unitOfWork.Warehouses.GetByIdAsync(dto.SourceWarehouseId)
            ?? throw new KeyNotFoundException($"Warehouse {dto.SourceWarehouseId} was not found.");
        var destinationWarehouse = await _unitOfWork.Warehouses.GetByIdAsync(dto.DestinationWarehouseId)
            ?? throw new KeyNotFoundException($"Warehouse {dto.DestinationWarehouseId} was not found.");
        if (dto.SourceWarehouseId == dto.DestinationWarehouseId)
            throw new DomainException("Source and destination warehouses must differ.");
        if (dto.Quantity <= 0)
            throw new DomainException("Transfer quantity must be positive.");

        // The source leg is where InsufficientStockException can be thrown. It runs
        // first and before anything is added to the change tracker, so a failed
        // transfer leaves zero pending changes - there is nothing for the
        // destination leg or SaveChangesAsync to partially commit.
        await AdjustStockAsync(dto.ProductId, dto.SourceWarehouseId, -dto.Quantity);
        await AdjustStockAsync(dto.ProductId, dto.DestinationWarehouseId, dto.Quantity);

        var reference = dto.Reference ?? $"TRANSFER-{Guid.NewGuid():N}";
        var occurredUtc = DateTime.UtcNow;

        var outMovement = new StockMovement
        {
            ProductId = dto.ProductId,
            Product = product,
            WarehouseId = dto.SourceWarehouseId,
            Warehouse = sourceWarehouse,
            RelatedWarehouseId = dto.DestinationWarehouseId,
            RelatedWarehouse = destinationWarehouse,
            Type = MovementType.Out,
            Quantity = dto.Quantity,
            Reference = reference,
            Notes = dto.Notes,
            OccurredUtc = occurredUtc,
        };
        var inMovement = new StockMovement
        {
            ProductId = dto.ProductId,
            Product = product,
            WarehouseId = dto.DestinationWarehouseId,
            Warehouse = destinationWarehouse,
            RelatedWarehouseId = dto.SourceWarehouseId,
            RelatedWarehouse = sourceWarehouse,
            Type = MovementType.In,
            Quantity = dto.Quantity,
            Reference = reference,
            Notes = dto.Notes,
            OccurredUtc = occurredUtc,
        };

        await _unitOfWork.StockMovements.AddAsync(outMovement);
        await _unitOfWork.StockMovements.AddAsync(inMovement);
        await _unitOfWork.SaveChangesAsync();

        return _mapper.Map<List<StockMovementDto>>(new[] { outMovement, inMovement });
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

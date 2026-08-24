using AutoMapper;
using InventorySystem.Application.DTOs;
using InventorySystem.Application.Interfaces;
using InventorySystem.Domain.Entities;
using InventorySystem.Domain.Enums;
using InventorySystem.Domain.Exceptions;
using InventorySystem.Domain.Interfaces;

namespace InventorySystem.Application.Services;

public class PurchaseOrderService : IPurchaseOrderService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IStockService _stockService;
    private readonly IMapper _mapper;

    public PurchaseOrderService(IUnitOfWork unitOfWork, IStockService stockService, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _stockService = stockService;
        _mapper = mapper;
    }

    public async Task<IReadOnlyList<PurchaseOrderDto>> GetAllAsync()
    {
        var orders = await _unitOfWork.PurchaseOrders.GetAllAsync("Supplier", "Items.Product");
        return _mapper.Map<List<PurchaseOrderDto>>(orders);
    }

    public async Task<PurchaseOrderDto?> GetByIdAsync(int id)
    {
        var order = await GetOrderWithItemsAsync(id);
        return order is null ? null : _mapper.Map<PurchaseOrderDto>(order);
    }

    public async Task<PurchaseOrderDto> CreateAsync(CreatePurchaseOrderDto dto)
    {
        if (await _unitOfWork.Suppliers.GetByIdAsync(dto.SupplierId) is null)
            throw new KeyNotFoundException($"Supplier {dto.SupplierId} was not found.");

        if (dto.Items.Count == 0)
            throw new DomainException("A purchase order must have at least one item.");

        var order = new PurchaseOrder
        {
            SupplierId = dto.SupplierId,
            ExpectedDeliveryDateUtc = dto.ExpectedDeliveryDateUtc,
            Notes = dto.Notes,
            Status = PurchaseOrderStatus.Draft,
            OrderDateUtc = DateTime.UtcNow,
        };

        foreach (var line in dto.Items)
        {
            if (line.QuantityOrdered <= 0)
                throw new DomainException("Ordered quantity must be positive.");
            if (await _unitOfWork.Products.GetByIdAsync(line.ProductId) is null)
                throw new KeyNotFoundException($"Product {line.ProductId} was not found.");

            order.Items.Add(new PurchaseOrderItem
            {
                ProductId = line.ProductId,
                QuantityOrdered = line.QuantityOrdered,
                UnitCost = line.UnitCost,
            });
        }

        await _unitOfWork.PurchaseOrders.AddAsync(order);
        await _unitOfWork.SaveChangesAsync();

        return _mapper.Map<PurchaseOrderDto>(order);
    }

    public async Task SendAsync(int id)
    {
        var order = await GetOrderOrThrowAsync(id);

        if (order.Status != PurchaseOrderStatus.Draft)
            throw new DomainException("Only draft purchase orders can be sent.");

        order.Status = PurchaseOrderStatus.Sent;
        _unitOfWork.PurchaseOrders.Update(order);
        await _unitOfWork.SaveChangesAsync();
    }

    public async Task CancelAsync(int id)
    {
        var order = await GetOrderOrThrowAsync(id);

        if (order.Status == PurchaseOrderStatus.Received)
            throw new DomainException("A received purchase order cannot be cancelled.");

        order.Status = PurchaseOrderStatus.Cancelled;
        _unitOfWork.PurchaseOrders.Update(order);
        await _unitOfWork.SaveChangesAsync();
    }

    public async Task ReceiveAsync(int id, ReceivePurchaseOrderDto dto)
    {
        var order = await GetOrderWithItemsAsync(id)
            ?? throw new KeyNotFoundException($"Purchase order {id} was not found.");

        if (order.Status != PurchaseOrderStatus.Sent)
            throw new DomainException("Only sent purchase orders can be received.");

        if (await _unitOfWork.Warehouses.GetByIdAsync(dto.WarehouseId) is null)
            throw new KeyNotFoundException($"Warehouse {dto.WarehouseId} was not found.");

        foreach (var line in dto.Items)
        {
            var item = order.Items.FirstOrDefault(i => i.Id == line.PurchaseOrderItemId)
                ?? throw new KeyNotFoundException($"Purchase order item {line.PurchaseOrderItemId} was not found on this order.");

            if (line.QuantityReceived <= 0)
                throw new DomainException("Received quantity must be positive.");

            var remaining = item.QuantityOrdered - item.QuantityReceived;
            if (line.QuantityReceived > remaining)
                throw new DomainException(
                    $"Cannot receive {line.QuantityReceived} units for product {item.ProductId}; only {remaining} remain on order.");

            item.QuantityReceived += line.QuantityReceived;

            // Stage only - no SaveChangesAsync here. All lines plus the
            // PurchaseOrder/Item updates below commit together in the single
            // SaveChangesAsync at the end of this method, so a bad line (or a
            // stock problem partway through a multi-line receipt) leaves the
            // whole receive rejected rather than half-applied.
            await _stockService.StageMovementAsync(new CreateStockMovementDto
            {
                ProductId = item.ProductId,
                WarehouseId = dto.WarehouseId,
                Type = MovementType.In,
                Quantity = line.QuantityReceived,
                Reference = $"PO-{order.Id}",
            });
        }

        if (order.Items.All(i => i.QuantityReceived >= i.QuantityOrdered))
            order.Status = PurchaseOrderStatus.Received;

        _unitOfWork.PurchaseOrders.Update(order);
        await _unitOfWork.SaveChangesAsync();
    }

    private Task<PurchaseOrder?> GetOrderWithItemsAsync(int id) =>
        _unitOfWork.PurchaseOrders.GetByIdAsync(id, "Supplier", "Items.Product");

    private async Task<PurchaseOrder> GetOrderOrThrowAsync(int id) =>
        await _unitOfWork.PurchaseOrders.GetByIdAsync(id)
            ?? throw new KeyNotFoundException($"Purchase order {id} was not found.");
}

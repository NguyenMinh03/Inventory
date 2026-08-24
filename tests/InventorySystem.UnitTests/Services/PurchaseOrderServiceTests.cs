using AutoMapper;
using InventorySystem.Application.DTOs;
using InventorySystem.Application.Interfaces;
using InventorySystem.Application.Mappings;
using InventorySystem.Application.Services;
using InventorySystem.Domain.Entities;
using InventorySystem.Domain.Enums;
using InventorySystem.Domain.Exceptions;
using InventorySystem.Domain.Interfaces;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;

namespace InventorySystem.UnitTests.Services;

public class PurchaseOrderServiceTests
{
    private readonly Mock<IUnitOfWork> _unitOfWork = new();
    private readonly Mock<IRepository<Supplier>> _suppliers = new();
    private readonly Mock<IRepository<Product>> _products = new();
    private readonly Mock<IRepository<Warehouse>> _warehouses = new();
    private readonly Mock<IRepository<PurchaseOrder>> _purchaseOrders = new();
    private readonly Mock<IStockService> _stockService = new();
    private readonly IMapper _mapper;

    public PurchaseOrderServiceTests()
    {
        var config = new MapperConfiguration(cfg => cfg.AddProfile<MappingProfile>(), NullLoggerFactory.Instance);
        _mapper = config.CreateMapper();

        _unitOfWork.SetupGet(u => u.Suppliers).Returns(_suppliers.Object);
        _unitOfWork.SetupGet(u => u.Products).Returns(_products.Object);
        _unitOfWork.SetupGet(u => u.Warehouses).Returns(_warehouses.Object);
        _unitOfWork.SetupGet(u => u.PurchaseOrders).Returns(_purchaseOrders.Object);

        _warehouses.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(new Warehouse { Id = 1, Name = "Main" });
    }

    private static PurchaseOrder MakeSentOrder(int orderId, int itemId, int productId, int quantityOrdered)
    {
        var order = new PurchaseOrder { Id = orderId, SupplierId = 1, Status = PurchaseOrderStatus.Sent };
        order.Items.Add(new PurchaseOrderItem
        {
            Id = itemId,
            PurchaseOrderId = orderId,
            ProductId = productId,
            QuantityOrdered = quantityOrdered,
            QuantityReceived = 0,
        });
        return order;
    }

    [Fact]
    public async Task ReceiveAsync_FullQuantity_StagesMovementAndMarksOrderReceived()
    {
        var order = MakeSentOrder(orderId: 1, itemId: 10, productId: 5, quantityOrdered: 20);
        _purchaseOrders.Setup(r => r.GetByIdAsync(1, "Supplier", "Items.Product")).ReturnsAsync(order);

        CreateStockMovementDto? staged = null;
        _stockService.Setup(s => s.StageMovementAsync(It.IsAny<CreateStockMovementDto>()))
            .Callback<CreateStockMovementDto>(dto => staged = dto)
            .ReturnsAsync(new StockMovementDto());

        var service = new PurchaseOrderService(_unitOfWork.Object, _stockService.Object, _mapper);
        var dto = new ReceivePurchaseOrderDto
        {
            WarehouseId = 1,
            Items = [new ReceivePurchaseOrderItemDto { PurchaseOrderItemId = 10, QuantityReceived = 20 }],
        };

        await service.ReceiveAsync(1, dto);

        Assert.NotNull(staged);
        Assert.Equal(5, staged!.ProductId);
        Assert.Equal(1, staged.WarehouseId);
        Assert.Equal(MovementType.In, staged.Type);
        Assert.Equal(20, staged.Quantity);
        Assert.Equal("PO-1", staged.Reference);

        Assert.Equal(20, order.Items.Single().QuantityReceived);
        Assert.Equal(PurchaseOrderStatus.Received, order.Status);

        // The stock movement is only staged here - PurchaseOrderService owns the
        // single SaveChangesAsync that commits it together with the PO update.
        _stockService.Verify(s => s.RecordMovementAsync(It.IsAny<CreateStockMovementDto>()), Times.Never);
        _unitOfWork.Verify(u => u.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task ReceiveAsync_PartialQuantity_LeavesOrderSent()
    {
        var order = MakeSentOrder(orderId: 2, itemId: 11, productId: 5, quantityOrdered: 20);
        _purchaseOrders.Setup(r => r.GetByIdAsync(2, "Supplier", "Items.Product")).ReturnsAsync(order);
        _stockService.Setup(s => s.StageMovementAsync(It.IsAny<CreateStockMovementDto>()))
            .ReturnsAsync(new StockMovementDto());

        var service = new PurchaseOrderService(_unitOfWork.Object, _stockService.Object, _mapper);
        var dto = new ReceivePurchaseOrderDto
        {
            WarehouseId = 1,
            Items = [new ReceivePurchaseOrderItemDto { PurchaseOrderItemId = 11, QuantityReceived = 8 }],
        };

        await service.ReceiveAsync(2, dto);

        Assert.Equal(8, order.Items.Single().QuantityReceived);
        Assert.Equal(PurchaseOrderStatus.Sent, order.Status);
    }

    [Fact]
    public async Task ReceiveAsync_QuantityExceedingRemaining_ThrowsDomainExceptionAndStagesNothing()
    {
        var order = MakeSentOrder(orderId: 3, itemId: 12, productId: 5, quantityOrdered: 10);
        _purchaseOrders.Setup(r => r.GetByIdAsync(3, "Supplier", "Items.Product")).ReturnsAsync(order);

        var service = new PurchaseOrderService(_unitOfWork.Object, _stockService.Object, _mapper);
        var dto = new ReceivePurchaseOrderDto
        {
            WarehouseId = 1,
            Items = [new ReceivePurchaseOrderItemDto { PurchaseOrderItemId = 12, QuantityReceived = 999 }],
        };

        await Assert.ThrowsAsync<DomainException>(() => service.ReceiveAsync(3, dto));

        _stockService.Verify(s => s.StageMovementAsync(It.IsAny<CreateStockMovementDto>()), Times.Never);
        _unitOfWork.Verify(u => u.SaveChangesAsync(), Times.Never);
    }

    [Fact]
    public async Task ReceiveAsync_OrderNotSent_ThrowsDomainException()
    {
        var order = MakeSentOrder(orderId: 4, itemId: 13, productId: 5, quantityOrdered: 10);
        order.Status = PurchaseOrderStatus.Draft;
        _purchaseOrders.Setup(r => r.GetByIdAsync(4, "Supplier", "Items.Product")).ReturnsAsync(order);

        var service = new PurchaseOrderService(_unitOfWork.Object, _stockService.Object, _mapper);
        var dto = new ReceivePurchaseOrderDto
        {
            WarehouseId = 1,
            Items = [new ReceivePurchaseOrderItemDto { PurchaseOrderItemId = 13, QuantityReceived = 5 }],
        };

        await Assert.ThrowsAsync<DomainException>(() => service.ReceiveAsync(4, dto));
    }
}

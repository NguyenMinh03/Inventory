using AutoMapper;
using InventorySystem.Application.DTOs;
using InventorySystem.Application.Mappings;
using InventorySystem.Application.Services;
using InventorySystem.Domain.Entities;
using InventorySystem.Domain.Enums;
using InventorySystem.Domain.Exceptions;
using InventorySystem.Domain.Interfaces;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;

namespace InventorySystem.UnitTests.Services;

public class StockServiceTests
{
    private readonly Mock<IUnitOfWork> _unitOfWork = new();
    private readonly Mock<IRepository<Product>> _products = new();
    private readonly Mock<IRepository<Warehouse>> _warehouses = new();
    private readonly Mock<IStockLevelRepository> _stockLevels = new();
    private readonly Mock<IRepository<StockMovement>> _stockMovements = new();
    private readonly IMapper _mapper;

    public StockServiceTests()
    {
        var config = new MapperConfiguration(cfg => cfg.AddProfile<MappingProfile>(), NullLoggerFactory.Instance);
        _mapper = config.CreateMapper();

        _unitOfWork.SetupGet(u => u.Products).Returns(_products.Object);
        _unitOfWork.SetupGet(u => u.Warehouses).Returns(_warehouses.Object);
        _unitOfWork.SetupGet(u => u.StockLevels).Returns(_stockLevels.Object);
        _unitOfWork.SetupGet(u => u.StockMovements).Returns(_stockMovements.Object);

        _products.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(new Product("SKU-1", "Widget", "each", 9.99m, 5, 1));
        _warehouses.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(new Warehouse { Id = 1, Name = "Main" });
    }

    [Fact]
    public async Task RecordMovementAsync_OutWithNoStockOnHand_ThrowsInsufficientStockException()
    {
        _stockLevels.Setup(r => r.GetByIdAsync(1, 1)).ReturnsAsync((StockLevel?)null);

        var service = new StockService(_unitOfWork.Object, _mapper);
        var dto = new CreateStockMovementDto { ProductId = 1, WarehouseId = 1, Type = MovementType.Out, Quantity = 10 };

        await Assert.ThrowsAsync<InsufficientStockException>(() => service.RecordMovementAsync(dto));
    }

    [Fact]
    public async Task RecordMovementAsync_OutExceedingOnHandQuantity_ThrowsInsufficientStockException()
    {
        _stockLevels.Setup(r => r.GetByIdAsync(1, 1))
            .ReturnsAsync(new StockLevel { ProductId = 1, WarehouseId = 1, QuantityOnHand = 5 });

        var service = new StockService(_unitOfWork.Object, _mapper);
        var dto = new CreateStockMovementDto { ProductId = 1, WarehouseId = 1, Type = MovementType.Out, Quantity = 10 };

        await Assert.ThrowsAsync<InsufficientStockException>(() => service.RecordMovementAsync(dto));
    }

    [Fact]
    public async Task RecordMovementAsync_InWithNoExistingStockLevel_CreatesAndIncreasesIt()
    {
        _stockLevels.Setup(r => r.GetByIdAsync(1, 1)).ReturnsAsync((StockLevel?)null);

        StockLevel? added = null;
        _stockLevels.Setup(r => r.AddAsync(It.IsAny<StockLevel>()))
            .Callback<StockLevel>(s => added = s)
            .Returns(Task.CompletedTask);

        var service = new StockService(_unitOfWork.Object, _mapper);
        var dto = new CreateStockMovementDto { ProductId = 1, WarehouseId = 1, Type = MovementType.In, Quantity = 25 };

        var result = await service.RecordMovementAsync(dto);

        Assert.NotNull(added);
        Assert.Equal(25, added!.QuantityOnHand);
        Assert.Equal(MovementType.In, result.Type);
        _stockMovements.Verify(r => r.AddAsync(It.IsAny<StockMovement>()), Times.Once);
        _unitOfWork.Verify(u => u.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task RecordMovementAsync_TransferWithoutDestinationWarehouse_ThrowsDomainException()
    {
        var service = new StockService(_unitOfWork.Object, _mapper);
        var dto = new CreateStockMovementDto { ProductId = 1, WarehouseId = 1, Type = MovementType.Transfer, Quantity = 5 };

        await Assert.ThrowsAsync<DomainException>(() => service.RecordMovementAsync(dto));
    }
}

using AutoMapper;
using InventorySystem.Application.DTOs;
using InventorySystem.Application.Mappings;
using InventorySystem.Application.Services;
using InventorySystem.Domain.Entities;
using InventorySystem.Domain.Interfaces;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;

namespace InventorySystem.UnitTests.Services;

public class ProductServiceTests
{
    private readonly Mock<IUnitOfWork> _unitOfWork = new();
    private readonly Mock<IRepository<Product>> _products = new();
    private readonly Mock<IRepository<Category>> _categories = new();
    private readonly IMapper _mapper;

    public ProductServiceTests()
    {
        var config = new MapperConfiguration(cfg => cfg.AddProfile<MappingProfile>(), NullLoggerFactory.Instance);
        _mapper = config.CreateMapper();

        _unitOfWork.SetupGet(u => u.Products).Returns(_products.Object);
        _unitOfWork.SetupGet(u => u.Categories).Returns(_categories.Object);
    }

    [Fact]
    public async Task CreateAsync_WithValidCategory_AddsProductAndSaves()
    {
        _categories.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(new Category { Id = 1, Name = "Electronics" });

        var service = new ProductService(_unitOfWork.Object, _mapper);
        var dto = new CreateProductDto
        {
            Sku = "SKU-1",
            Name = "Widget",
            UnitOfMeasure = "each",
            UnitPrice = 9.99m,
            ReorderLevel = 5,
            CategoryId = 1,
        };

        var result = await service.CreateAsync(dto);

        Assert.Equal("SKU-1", result.Sku);
        _products.Verify(r => r.AddAsync(It.Is<Product>(p => p.Sku == "SKU-1")), Times.Once);
        _unitOfWork.Verify(u => u.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task CreateAsync_WithUnknownCategory_ThrowsKeyNotFoundException()
    {
        _categories.Setup(r => r.GetByIdAsync(99)).ReturnsAsync((Category?)null);

        var service = new ProductService(_unitOfWork.Object, _mapper);
        var dto = new CreateProductDto
        {
            Sku = "SKU-1",
            Name = "Widget",
            UnitOfMeasure = "each",
            UnitPrice = 9.99m,
            ReorderLevel = 5,
            CategoryId = 99,
        };

        await Assert.ThrowsAsync<KeyNotFoundException>(() => service.CreateAsync(dto));
        _products.Verify(r => r.AddAsync(It.IsAny<Product>()), Times.Never);
    }
}

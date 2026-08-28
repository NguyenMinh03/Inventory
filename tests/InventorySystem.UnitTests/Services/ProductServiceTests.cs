using InventorySystem.Application.DTOs;
using InventorySystem.Application.Services;
using InventorySystem.Domain.Entities;
using InventorySystem.Domain.Interfaces;
using Moq;

namespace InventorySystem.UnitTests.Services;

[Collection(MapperCollection.Name)]
public class ProductServiceTests : ServiceTestBase
{
    private readonly Mock<IRepository<Product>> _products = new();
    private readonly Mock<IRepository<Category>> _categories = new();

    public ProductServiceTests(MapperFixture mapperFixture) : base(mapperFixture)
    {
        UnitOfWork.SetupGet(u => u.Products).Returns(_products.Object);
        UnitOfWork.SetupGet(u => u.Categories).Returns(_categories.Object);
    }

    [Fact]
    public async Task CreateAsync_WithValidCategory_AddsProductAndSaves()
    {
        _categories.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(new Category { Id = 1, Name = "Electronics" });

        var service = new ProductService(UnitOfWork.Object, Mapper);
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
        UnitOfWork.Verify(u => u.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task CreateAsync_WithUnknownCategory_ThrowsKeyNotFoundException()
    {
        _categories.Setup(r => r.GetByIdAsync(99)).ReturnsAsync((Category?)null);

        var service = new ProductService(UnitOfWork.Object, Mapper);
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

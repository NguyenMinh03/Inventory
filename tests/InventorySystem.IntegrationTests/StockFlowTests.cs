using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using InventorySystem.Application.DTOs;
using InventorySystem.Domain.Enums;

namespace InventorySystem.IntegrationTests;

public class StockFlowTests : IClassFixture<InventoryApiFactory>
{
    private readonly InventoryApiFactory _factory;

    public StockFlowTests(InventoryApiFactory factory)
    {
        _factory = factory;
    }

    private async Task<HttpClient> CreateAuthenticatedClientAsync(string username = "admin", string password = "Admin123!")
    {
        var client = _factory.CreateClient();

        var loginResponse = await client.PostAsJsonAsync("/api/auth/login", new LoginDto { Username = username, Password = password });
        loginResponse.EnsureSuccessStatusCode();
        var auth = await loginResponse.Content.ReadFromJsonAsync<AuthResultDto>();

        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", auth!.Token);
        return client;
    }

    [Fact]
    public async Task CreateProduct_RecordStockIn_ThenGetStockLevel_ReflectsRecordedQuantity()
    {
        var client = await CreateAuthenticatedClientAsync();

        // Arrange: the seed guarantees at least one category and one warehouse exist.
        var categories = await (await client.GetAsync("/api/Categories")).Content.ReadFromJsonAsync<List<CategoryDto>>();
        var warehouses = await (await client.GetAsync("/api/Warehouses")).Content.ReadFromJsonAsync<List<WarehouseDto>>();
        var categoryId = categories!.First().Id;
        var warehouseId = warehouses!.First().Id;

        // Act 1: create product.
        var createResponse = await client.PostAsJsonAsync("/api/Products", new CreateProductDto
        {
            Sku = "IT-SKU-0001",
            Name = "Integration Test Widget",
            UnitOfMeasure = "each",
            UnitPrice = 9.99m,
            ReorderLevel = 5,
            CategoryId = categoryId,
        });
        Assert.Equal(HttpStatusCode.Created, createResponse.StatusCode);
        var product = await createResponse.Content.ReadFromJsonAsync<ProductDto>();
        Assert.NotNull(product);

        // Act 2: record 40 units in.
        var movementResponse = await client.PostAsJsonAsync("/api/stock/movements", new CreateStockMovementDto
        {
            ProductId = product!.Id,
            WarehouseId = warehouseId,
            Type = MovementType.In,
            Quantity = 40,
        });
        Assert.Equal(HttpStatusCode.OK, movementResponse.StatusCode);

        // Assert: the stock level GET reflects exactly what was recorded.
        var levelsResponse = await client.GetAsync($"/api/stock/levels/product/{product.Id}");
        Assert.Equal(HttpStatusCode.OK, levelsResponse.StatusCode);
        var levels = await levelsResponse.Content.ReadFromJsonAsync<List<StockLevelDto>>();

        var levelAtWarehouse = Assert.Single(levels!, l => l.WarehouseId == warehouseId);
        Assert.Equal(40, levelAtWarehouse.QuantityOnHand);
        Assert.Equal(product.Id, levelAtWarehouse.ProductId);
    }

    [Fact]
    public async Task RecordStockOut_ExceedingOnHand_Returns409AndLeavesLevelUnchanged()
    {
        var client = await CreateAuthenticatedClientAsync();

        var categories = await (await client.GetAsync("/api/Categories")).Content.ReadFromJsonAsync<List<CategoryDto>>();
        var warehouses = await (await client.GetAsync("/api/Warehouses")).Content.ReadFromJsonAsync<List<WarehouseDto>>();

        var createResponse = await client.PostAsJsonAsync("/api/Products", new CreateProductDto
        {
            Sku = "IT-SKU-0002",
            Name = "Integration Test Gadget",
            UnitOfMeasure = "each",
            UnitPrice = 5.00m,
            ReorderLevel = 1,
            CategoryId = categories!.First().Id,
        });
        var product = await createResponse.Content.ReadFromJsonAsync<ProductDto>();
        var warehouseId = warehouses!.First().Id;

        await client.PostAsJsonAsync("/api/stock/movements", new CreateStockMovementDto
        {
            ProductId = product!.Id,
            WarehouseId = warehouseId,
            Type = MovementType.In,
            Quantity = 10,
        });

        // Try to take out far more than is on hand.
        var outResponse = await client.PostAsJsonAsync("/api/stock/movements", new CreateStockMovementDto
        {
            ProductId = product.Id,
            WarehouseId = warehouseId,
            Type = MovementType.Out,
            Quantity = 999,
        });
        Assert.Equal(HttpStatusCode.Conflict, outResponse.StatusCode);

        var levels = await (await client.GetAsync($"/api/stock/levels/product/{product.Id}"))
            .Content.ReadFromJsonAsync<List<StockLevelDto>>();
        var levelAtWarehouse = Assert.Single(levels!, l => l.WarehouseId == warehouseId);
        Assert.Equal(10, levelAtWarehouse.QuantityOnHand);
    }

    [Fact]
    public async Task GetProducts_WithoutAuthentication_Returns401()
    {
        var client = _factory.CreateClient();

        var response = await client.GetAsync("/api/Products");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task DeleteProduct_AsStaff_Returns403()
    {
        var adminClient = await CreateAuthenticatedClientAsync();
        var categories = await (await adminClient.GetAsync("/api/Categories")).Content.ReadFromJsonAsync<List<CategoryDto>>();

        var createResponse = await adminClient.PostAsJsonAsync("/api/Products", new CreateProductDto
        {
            Sku = "IT-SKU-0003",
            Name = "Integration Test Delete Target",
            UnitOfMeasure = "each",
            UnitPrice = 1.00m,
            ReorderLevel = 1,
            CategoryId = categories!.First().Id,
        });
        var product = await createResponse.Content.ReadFromJsonAsync<ProductDto>();

        var staffClient = await CreateAuthenticatedClientAsync("staff", "Staff123!");

        var deleteResponse = await staffClient.DeleteAsync($"/api/Products/{product!.Id}");

        Assert.Equal(HttpStatusCode.Forbidden, deleteResponse.StatusCode);
    }
}

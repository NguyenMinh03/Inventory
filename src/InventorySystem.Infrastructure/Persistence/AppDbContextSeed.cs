using InventorySystem.Domain.Entities;
using InventorySystem.Domain.Enums;
using InventorySystem.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace InventorySystem.Infrastructure.Persistence;

public static class AppDbContextSeed
{
    public static async Task SeedAsync(AppDbContext context, IPasswordHasher passwordHasher)
    {
        // Guarded independently of the catalog seed below: the Users table was
        // added in a later migration, so a database that already has categories
        // from an earlier run would otherwise never get users seeded.
        if (!await context.Users.AnyAsync())
        {
            context.Users.AddRange(
                new User { Username = "admin", PasswordHash = passwordHasher.Hash("Admin123!"), Role = UserRole.Admin },
                new User { Username = "manager", PasswordHash = passwordHasher.Hash("Manager123!"), Role = UserRole.Manager },
                new User { Username = "staff", PasswordHash = passwordHasher.Hash("Staff123!"), Role = UserRole.Staff });

            await context.SaveChangesAsync();
        }

        if (await context.Categories.AnyAsync())
            return;

        var electronics = new Category { Name = "Electronics", Description = "Electronic components and devices" };
        var officeSupplies = new Category { Name = "Office Supplies", Description = "General office consumables" };
        context.Categories.AddRange(electronics, officeSupplies);

        var mainWarehouse = new Warehouse { Name = "Main Warehouse", Address = "100 Logistics Way, Springfield" };
        var overflowWarehouse = new Warehouse { Name = "Overflow Warehouse", Address = "22 Industrial Park Rd, Springfield" };
        context.Warehouses.AddRange(mainWarehouse, overflowWarehouse);

        var supplier = new Supplier { Name = "Acme Distribution Co.", ContactName = "Jordan Lee", Email = "orders@acmedist.example" };
        context.Suppliers.Add(supplier);

        await context.SaveChangesAsync();

        var products = new List<Product>
        {
            new("SKU-1001", "USB-C Cable 1m", "each", 4.99m, 50, electronics.Id),
            new("SKU-1002", "Wireless Mouse", "each", 14.99m, 30, electronics.Id),
            new("SKU-1003", "27-inch Monitor", "each", 189.00m, 10, electronics.Id),
            new("SKU-2001", "A4 Paper Ream", "ream", 3.49m, 100, officeSupplies.Id),
            new("SKU-2002", "Ballpoint Pen (Box of 12)", "box", 5.25m, 40, officeSupplies.Id),
        };
        context.Products.AddRange(products);

        await context.SaveChangesAsync();

        foreach (var product in products)
        {
            context.StockLevels.Add(new StockLevel { ProductId = product.Id, WarehouseId = mainWarehouse.Id, QuantityOnHand = 75 });
            context.StockLevels.Add(new StockLevel { ProductId = product.Id, WarehouseId = overflowWarehouse.Id, QuantityOnHand = 20 });
        }

        await context.SaveChangesAsync();
    }
}

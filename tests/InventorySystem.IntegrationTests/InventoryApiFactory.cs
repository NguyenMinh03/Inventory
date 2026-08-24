using InventorySystem.Domain.Interfaces;
using InventorySystem.Infrastructure.Persistence;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace InventorySystem.IntegrationTests;

// Points the API at a dedicated LocalDB database rather than an in-memory or
// SQLite provider. The app relies on real SQL Server behavior in a couple of
// places - ReportRepository's movement-history query casts
// Database.GetDbConnection() to SqlConnection and hand-writes T-SQL
// (OFFSET/FETCH), and several columns use HasConversion<string>() - so a
// non-SQL-Server provider would either fail outright or silently diverge from
// what actually runs in dev/prod. LocalDB is already this project's whole
// data-layer choice (see Phase 2), needs no Docker, and is fast to spin up
// fresh per test run via EnsureDeleted + Migrate.
public class InventoryApiFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    public const string ConnectionString =
        "Server=(localdb)\\MSSQLLocalDB;Database=InventorySystemDb_IntegrationTests;Trusted_Connection=True;TrustServerCertificate=True;MultipleActiveResultSets=true";

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");

        builder.ConfigureAppConfiguration((_, config) =>
        {
            config.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ConnectionStrings:Default"] = ConnectionString,
                ["Jwt:Key"] = "Integration-Test-Only-Signing-Key-Not-Used-Anywhere-Else-1234567890",
                ["Jwt:Issuer"] = "InventorySystem.IntegrationTests",
                ["Jwt:Audience"] = "InventorySystem.IntegrationTests",
                ["Jwt:ExpiryMinutes"] = "60",
            });
        });
    }

    public async Task InitializeAsync()
    {
        using var scope = Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var passwordHasher = scope.ServiceProvider.GetRequiredService<IPasswordHasher>();

        await db.Database.EnsureDeletedAsync();
        await db.Database.MigrateAsync();
        await AppDbContextSeed.SeedAsync(db, passwordHasher);
    }

    async Task IAsyncLifetime.DisposeAsync()
    {
        using var scope = Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        await db.Database.EnsureDeletedAsync();

        await base.DisposeAsync();
    }
}

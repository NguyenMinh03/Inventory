using System.Data;
using InventorySystem.Domain.Interfaces;
using InventorySystem.Domain.Reporting;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace InventorySystem.Infrastructure.Persistence.Repositories;

public class ReportRepository : IReportRepository
{
    private readonly AppDbContext _context;

    public ReportRepository(AppDbContext context)
    {
        _context = context;
    }

    // Plain LINQ - EF translates this into a GROUP BY/SUM query server-side.
    public async Task<IReadOnlyList<StockValuationRow>> GetStockValuationByWarehouseAsync()
    {
        return await _context.StockLevels
            .GroupBy(s => new { s.WarehouseId, s.Warehouse!.Name })
            .Select(g => new StockValuationRow
            {
                GroupId = g.Key.WarehouseId,
                GroupName = g.Key.Name,
                TotalQuantityOnHand = g.Sum(s => s.QuantityOnHand),
                TotalValue = g.Sum(s => s.QuantityOnHand * s.Product!.UnitPrice),
            })
            .OrderBy(r => r.GroupName)
            .ToListAsync();
    }

    public async Task<IReadOnlyList<StockValuationRow>> GetStockValuationByCategoryAsync()
    {
        return await _context.StockLevels
            .GroupBy(s => new { s.Product!.CategoryId, CategoryName = s.Product!.Category!.Name })
            .Select(g => new StockValuationRow
            {
                GroupId = g.Key.CategoryId,
                GroupName = g.Key.CategoryName,
                TotalQuantityOnHand = g.Sum(s => s.QuantityOnHand),
                TotalValue = g.Sum(s => s.QuantityOnHand * s.Product!.UnitPrice),
            })
            .OrderBy(r => r.GroupName)
            .ToListAsync();
    }

    // Written as hand-rolled parameterized SQL over ADO.NET (not FromSqlRaw,
    // not LINQ) to show that comfort directly: dynamic WHERE clause built from
    // optional filters, a join across three tables, and SQL Server's
    // OFFSET/FETCH pagination. Every value is bound through SqlParameter, so
    // this is not vulnerable to injection despite the string-built clause list -
    // the clause list only ever contains fixed column-name fragments written by
    // this method, never request input.
    public async Task<(IReadOnlyList<MovementHistoryRow> Items, int TotalCount)> GetMovementHistoryAsync(
        DateTime? from, DateTime? to, int? productId, int page, int pageSize)
    {
        var whereClauses = new List<string>();
        var paramValues = new List<(string Name, object Value)>();

        if (from is not null)
        {
            whereClauses.Add("m.OccurredUtc >= @From");
            paramValues.Add(("@From", from.Value));
        }
        if (to is not null)
        {
            whereClauses.Add("m.OccurredUtc <= @To");
            paramValues.Add(("@To", to.Value));
        }
        if (productId is not null)
        {
            whereClauses.Add("m.ProductId = @ProductId");
            paramValues.Add(("@ProductId", productId.Value));
        }

        var whereSql = whereClauses.Count > 0 ? "WHERE " + string.Join(" AND ", whereClauses) : string.Empty;

        var connection = (SqlConnection)_context.Database.GetDbConnection();
        var wasClosed = connection.State != ConnectionState.Open;
        if (wasClosed)
            await connection.OpenAsync();

        try
        {
            var totalCount = await ExecuteCountAsync(connection, whereSql, paramValues);
            var items = await ExecutePageAsync(connection, whereSql, paramValues, page, pageSize);
            return (items, totalCount);
        }
        finally
        {
            if (wasClosed)
                await connection.CloseAsync();
        }
    }

    private static async Task<int> ExecuteCountAsync(
        SqlConnection connection, string whereSql, List<(string Name, object Value)> paramValues)
    {
        await using var cmd = connection.CreateCommand();
        cmd.CommandText = $"SELECT COUNT(*) FROM StockMovements m {whereSql}";
        AddParameters(cmd, paramValues);

        var result = await cmd.ExecuteScalarAsync();
        return Convert.ToInt32(result);
    }

    private static async Task<List<MovementHistoryRow>> ExecutePageAsync(
        SqlConnection connection, string whereSql, List<(string Name, object Value)> paramValues, int page, int pageSize)
    {
        await using var cmd = connection.CreateCommand();
        cmd.CommandText = $"""
            SELECT m.Id, m.ProductId, p.Sku, p.Name,
                   m.WarehouseId, w.Name,
                   m.Type, m.Quantity, m.OccurredUtc, m.Reference
            FROM StockMovements m
            JOIN Products p ON p.Id = m.ProductId
            JOIN Warehouses w ON w.Id = m.WarehouseId
            {whereSql}
            ORDER BY m.OccurredUtc DESC
            OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY
            """;

        AddParameters(cmd, paramValues);
        cmd.Parameters.Add(new SqlParameter("@Offset", Math.Max(0, page - 1) * pageSize));
        cmd.Parameters.Add(new SqlParameter("@PageSize", pageSize));

        var items = new List<MovementHistoryRow>();
        await using var reader = await cmd.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            items.Add(new MovementHistoryRow
            {
                Id = reader.GetInt32(0),
                ProductId = reader.GetInt32(1),
                ProductSku = reader.GetString(2),
                ProductName = reader.GetString(3),
                WarehouseId = reader.GetInt32(4),
                WarehouseName = reader.GetString(5),
                // StockMovementConfiguration stores Type via HasConversion<string>(),
                // so the column is already the enum's name (e.g. "In"), not an int.
                Type = reader.GetString(6),
                Quantity = reader.GetInt32(7),
                OccurredUtc = reader.GetDateTime(8),
                Reference = reader.IsDBNull(9) ? null : reader.GetString(9),
            });
        }

        return items;
    }

    private static void AddParameters(SqlCommand cmd, List<(string Name, object Value)> paramValues)
    {
        foreach (var (name, value) in paramValues)
            cmd.Parameters.Add(new SqlParameter(name, value));
    }
}

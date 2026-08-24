namespace InventorySystem.Domain.Reporting;

// A query projection, not an entity: one aggregated row per warehouse or
// per category, depending on which IReportRepository method produced it.
public class StockValuationRow
{
    public int GroupId { get; set; }
    public string GroupName { get; set; } = string.Empty;
    public int TotalQuantityOnHand { get; set; }
    public decimal TotalValue { get; set; }
}

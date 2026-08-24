import { useState } from "react";
import { reportsApi } from "../api/endpoints";
import { useAsync } from "../hooks/useAsync";
import { ErrorBanner, Spinner, EmptyState } from "../components/Feedback";

type Tab = "low-stock" | "valuation" | "history";

export function ReportsPage() {
  const [tab, setTab] = useState<Tab>("low-stock");

  return (
    <div>
      <div className="page-header">
        <h1>Reports</h1>
      </div>

      <div className="tabs">
        <button className={"tab" + (tab === "low-stock" ? " active" : "")} onClick={() => setTab("low-stock")}>
          Low Stock
        </button>
        <button className={"tab" + (tab === "valuation" ? " active" : "")} onClick={() => setTab("valuation")}>
          Stock Valuation
        </button>
        <button className={"tab" + (tab === "history" ? " active" : "")} onClick={() => setTab("history")}>
          Movement History
        </button>
      </div>

      {tab === "low-stock" && <LowStockReport />}
      {tab === "valuation" && <ValuationReport />}
      {tab === "history" && <MovementHistoryReport />}
    </div>
  );
}

function LowStockReport() {
  const { data, loading, error } = useAsync(() => reportsApi.lowStock(), []);

  if (loading) return <Spinner />;
  if (error) return <ErrorBanner message={error} />;
  if (!data || data.length === 0) return <EmptyState message="Nothing is below its reorder level right now." />;

  return (
    <table className="data-table">
      <thead>
        <tr>
          <th>SKU</th>
          <th>Name</th>
          <th>Reorder level</th>
          <th>Category</th>
        </tr>
      </thead>
      <tbody>
        {data.map((p) => (
          <tr key={p.id}>
            <td>{p.sku}</td>
            <td>{p.name}</td>
            <td>{p.reorderLevel}</td>
            <td>{p.categoryName ?? "—"}</td>
          </tr>
        ))}
      </tbody>
    </table>
  );
}

function ValuationReport() {
  const [by, setBy] = useState<"warehouse" | "category">("warehouse");
  const { data, loading, error } = useAsync(() => reportsApi.stockValuation(by), [by]);

  return (
    <div>
      <div className="toolbar">
        <select value={by} onChange={(e) => setBy(e.target.value as "warehouse" | "category")}>
          <option value="warehouse">Group by warehouse</option>
          <option value="category">Group by category</option>
        </select>
      </div>

      {loading && <Spinner />}
      {error && <ErrorBanner message={error} />}

      {!loading && data && (
        <table className="data-table">
          <thead>
            <tr>
              <th>{by === "warehouse" ? "Warehouse" : "Category"}</th>
              <th>Total quantity on hand</th>
              <th>Total value</th>
            </tr>
          </thead>
          <tbody>
            {data.map((row) => (
              <tr key={row.groupId}>
                <td>{row.groupName}</td>
                <td>{row.totalQuantityOnHand}</td>
                <td>${row.totalValue.toFixed(2)}</td>
              </tr>
            ))}
          </tbody>
        </table>
      )}
    </div>
  );
}

function MovementHistoryReport() {
  const [page, setPage] = useState(1);
  const { data, loading, error } = useAsync(() => reportsApi.movementHistory({ page, pageSize: 15 }), [page]);

  return (
    <div>
      {loading && <Spinner />}
      {error && <ErrorBanner message={error} />}

      {!loading && data && data.items.length === 0 && <EmptyState message="No stock movements recorded yet." />}

      {!loading && data && data.items.length > 0 && (
        <>
          <table className="data-table">
            <thead>
              <tr>
                <th>When</th>
                <th>Product</th>
                <th>Warehouse</th>
                <th>Type</th>
                <th>Quantity</th>
                <th>Reference</th>
              </tr>
            </thead>
            <tbody>
              {data.items.map((m) => (
                <tr key={m.id}>
                  <td>{new Date(m.occurredUtc).toLocaleString()}</td>
                  <td>
                    {m.productSku} — {m.productName}
                  </td>
                  <td>{m.warehouseName}</td>
                  <td>{m.type}</td>
                  <td>{m.quantity}</td>
                  <td>{m.reference ?? "—"}</td>
                </tr>
              ))}
            </tbody>
          </table>

          <div className="pagination">
            <button className="btn btn-small" disabled={page <= 1} onClick={() => setPage((p) => p - 1)}>
              Previous
            </button>
            <span>
              Page {data.page} of {data.totalPages} ({data.totalCount} total)
            </span>
            <button
              className="btn btn-small"
              disabled={page >= data.totalPages}
              onClick={() => setPage((p) => p + 1)}
            >
              Next
            </button>
          </div>
        </>
      )}
    </div>
  );
}

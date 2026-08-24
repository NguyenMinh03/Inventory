import { Link } from "react-router-dom";
import { productsApi, reportsApi, purchaseOrdersApi, stockApi } from "../api/endpoints";
import { useAsync } from "../hooks/useAsync";
import { Spinner } from "../components/Feedback";
import { useAuth } from "../auth/AuthContext";

export function DashboardPage() {
  const { user } = useAuth();

  const { data: productsResult, loading: loadingProducts } = useAsync(
    () => productsApi.getAll({ page: 1, pageSize: 1 }),
    [],
  );
  const { data: lowStock, loading: loadingLowStock } = useAsync(() => reportsApi.lowStock(), []);
  const { data: orders, loading: loadingOrders } = useAsync(() => purchaseOrdersApi.getAll(), []);
  const { data: levels, loading: loadingLevels } = useAsync(() => stockApi.getLevels(), []);

  const loading = loadingProducts || loadingLowStock || loadingOrders || loadingLevels;
  const openOrders = orders?.filter((o) => o.status === "Draft" || o.status === "Sent").length ?? 0;
  const totalUnitsOnHand = levels?.reduce((sum, l) => sum + l.quantityOnHand, 0) ?? 0;

  return (
    <div>
      <div className="page-header">
        <h1>Welcome, {user?.username}</h1>
      </div>

      {loading ? (
        <Spinner />
      ) : (
        <>
          <div className="stat-grid">
            <Link to="/products" className="stat-card">
              <div className="stat-value">{productsResult?.totalCount ?? 0}</div>
              <div className="stat-label">Products</div>
            </Link>
            <Link to="/reports" className="stat-card stat-card-warn">
              <div className="stat-value">{lowStock?.length ?? 0}</div>
              <div className="stat-label">Below reorder level</div>
            </Link>
            <Link to="/purchase-orders" className="stat-card">
              <div className="stat-value">{openOrders}</div>
              <div className="stat-label">Open purchase orders</div>
            </Link>
            <Link to="/stock" className="stat-card">
              <div className="stat-value">{totalUnitsOnHand}</div>
              <div className="stat-label">Total units on hand</div>
            </Link>
          </div>

          {lowStock && lowStock.length > 0 && (
            <div className="card">
              <h2>Products below reorder level</h2>
              <table className="data-table">
                <thead>
                  <tr>
                    <th>SKU</th>
                    <th>Name</th>
                    <th>Reorder level</th>
                  </tr>
                </thead>
                <tbody>
                  {lowStock.map((p) => (
                    <tr key={p.id}>
                      <td>{p.sku}</td>
                      <td>{p.name}</td>
                      <td>{p.reorderLevel}</td>
                    </tr>
                  ))}
                </tbody>
              </table>
            </div>
          )}
        </>
      )}
    </div>
  );
}

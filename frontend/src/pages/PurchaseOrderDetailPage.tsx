import { useState } from "react";
import { useNavigate, useParams } from "react-router-dom";
import { purchaseOrdersApi, warehousesApi } from "../api/endpoints";
import { ApiError } from "../api/client";
import { useAsync } from "../hooks/useAsync";
import { ErrorBanner, Spinner } from "../components/Feedback";
import { StatusBadge } from "../components/StatusBadge";

export function PurchaseOrderDetailPage() {
  const { id } = useParams<{ id: string }>();
  const orderId = Number(id);
  const navigate = useNavigate();

  const { data: order, loading, error, reload } = useAsync(() => purchaseOrdersApi.getById(orderId), [orderId]);
  const { data: warehouses } = useAsync(() => warehousesApi.getAll(), []);

  const [warehouseId, setWarehouseId] = useState("");
  const [receiveQuantities, setReceiveQuantities] = useState<Record<number, string>>({});
  const [actionError, setActionError] = useState<string | null>(null);
  const [busy, setBusy] = useState(false);

  if (loading) return <Spinner />;
  if (error) return <ErrorBanner message={error} />;
  if (!order) return null;

  const remaining = (itemId: number) => {
    const item = order.items.find((i) => i.id === itemId);
    if (!item) return 0;
    return item.quantityOrdered - item.quantityReceived;
  };

  const handleSend = async () => {
    setBusy(true);
    setActionError(null);
    try {
      await purchaseOrdersApi.send(order.id);
      reload();
    } catch (err) {
      setActionError(err instanceof ApiError ? err.message : "Failed to send order.");
    } finally {
      setBusy(false);
    }
  };

  const handleCancel = async () => {
    if (!confirm("Cancel this purchase order?")) return;
    setBusy(true);
    setActionError(null);
    try {
      await purchaseOrdersApi.cancel(order.id);
      reload();
    } catch (err) {
      setActionError(err instanceof ApiError ? err.message : "Failed to cancel order.");
    } finally {
      setBusy(false);
    }
  };

  const handleReceive = async () => {
    setBusy(true);
    setActionError(null);

    const items = order.items
      .map((item) => ({
        purchaseOrderItemId: item.id,
        quantityReceived: Number(receiveQuantities[item.id] ?? 0),
      }))
      .filter((line) => line.quantityReceived > 0);

    if (items.length === 0) {
      setActionError("Enter a quantity to receive for at least one line.");
      setBusy(false);
      return;
    }

    try {
      await purchaseOrdersApi.receive(order.id, { warehouseId: Number(warehouseId), items });
      setReceiveQuantities({});
      reload();
    } catch (err) {
      setActionError(err instanceof ApiError ? err.message : "Failed to receive order.");
    } finally {
      setBusy(false);
    }
  };

  return (
    <div>
      <button className="btn btn-ghost" onClick={() => navigate("/purchase-orders")}>
        ← Back to purchase orders
      </button>

      <div className="page-header">
        <h1>
          Purchase Order #{order.id} <StatusBadge status={order.status} />
        </h1>
      </div>

      <p>
        <strong>Supplier:</strong> {order.supplierName}
        <br />
        <strong>Order date:</strong> {new Date(order.orderDateUtc).toLocaleString()}
        {order.notes && (
          <>
            <br />
            <strong>Notes:</strong> {order.notes}
          </>
        )}
      </p>

      {actionError && <ErrorBanner message={actionError} />}

      <table className="data-table">
        <thead>
          <tr>
            <th>Product</th>
            <th>Ordered</th>
            <th>Received</th>
            <th>Unit cost</th>
            {order.status === "Sent" && <th>Receive now</th>}
          </tr>
        </thead>
        <tbody>
          {order.items.map((item) => (
            <tr key={item.id}>
              <td>{item.productName}</td>
              <td>{item.quantityOrdered}</td>
              <td>{item.quantityReceived}</td>
              <td>${item.unitCost.toFixed(2)}</td>
              {order.status === "Sent" && (
                <td>
                  <input
                    type="number"
                    min="0"
                    max={remaining(item.id)}
                    placeholder={`up to ${remaining(item.id)}`}
                    value={receiveQuantities[item.id] ?? ""}
                    onChange={(e) =>
                      setReceiveQuantities((prev) => ({ ...prev, [item.id]: e.target.value }))
                    }
                    style={{ width: 100 }}
                  />
                </td>
              )}
            </tr>
          ))}
        </tbody>
      </table>

      <div className="po-actions">
        {order.status === "Draft" && (
          <button className="btn btn-primary" disabled={busy} onClick={handleSend}>
            Send order
          </button>
        )}

        {order.status === "Sent" && (
          <div className="card" style={{ maxWidth: 420 }}>
            <h2>Receive stock</h2>
            <label>
              Into warehouse
              <select value={warehouseId} onChange={(e) => setWarehouseId(e.target.value)}>
                <option value="" disabled>
                  Select...
                </option>
                {warehouses?.map((w) => (
                  <option key={w.id} value={w.id}>
                    {w.name}
                  </option>
                ))}
              </select>
            </label>
            <div className="form-actions">
              <button className="btn btn-primary" disabled={busy || !warehouseId} onClick={handleReceive}>
                Receive
              </button>
            </div>
          </div>
        )}

        {(order.status === "Draft" || order.status === "Sent") && (
          <button className="btn btn-danger" disabled={busy} onClick={handleCancel}>
            Cancel order
          </button>
        )}
      </div>
    </div>
  );
}

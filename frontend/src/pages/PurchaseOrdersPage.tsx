import { useState, type FormEvent } from "react";
import { Link } from "react-router-dom";
import { purchaseOrdersApi, suppliersApi, productsApi } from "../api/endpoints";
import { ApiError } from "../api/client";
import { useAsync } from "../hooks/useAsync";
import { Modal } from "../components/Modal";
import { ErrorBanner, Spinner, EmptyState } from "../components/Feedback";
import { StatusBadge } from "../components/StatusBadge";

interface LineForm {
  productId: string;
  quantityOrdered: string;
  unitCost: string;
}

const emptyLine: LineForm = { productId: "", quantityOrdered: "1", unitCost: "0" };

export function PurchaseOrdersPage() {
  const { data: orders, loading, error, reload } = useAsync(() => purchaseOrdersApi.getAll(), []);
  const { data: suppliers } = useAsync(() => suppliersApi.getAll(), []);
  const { data: productsResult } = useAsync(() => productsApi.getAll({ page: 1, pageSize: 200 }), []);

  const [creating, setCreating] = useState(false);
  const [supplierId, setSupplierId] = useState("");
  const [notes, setNotes] = useState("");
  const [lines, setLines] = useState<LineForm[]>([{ ...emptyLine }]);
  const [formError, setFormError] = useState<string | null>(null);
  const [saving, setSaving] = useState(false);

  const products = productsResult?.items ?? [];

  const openCreate = () => {
    setSupplierId(suppliers?.[0]?.id.toString() ?? "");
    setNotes("");
    setLines([{ ...emptyLine }]);
    setFormError(null);
    setCreating(true);
  };

  const updateLine = (index: number, patch: Partial<LineForm>) => {
    setLines((prev) => prev.map((line, i) => (i === index ? { ...line, ...patch } : line)));
  };

  const handleSubmit = async (e: FormEvent) => {
    e.preventDefault();
    setSaving(true);
    setFormError(null);

    try {
      await purchaseOrdersApi.create({
        supplierId: Number(supplierId),
        notes: notes || null,
        items: lines.map((l) => ({
          productId: Number(l.productId),
          quantityOrdered: Number(l.quantityOrdered),
          unitCost: Number(l.unitCost),
        })),
      });
      setCreating(false);
      reload();
    } catch (err) {
      setFormError(err instanceof ApiError ? err.message : "Failed to create purchase order.");
    } finally {
      setSaving(false);
    }
  };

  return (
    <div>
      <div className="page-header">
        <h1>Purchase Orders</h1>
        <button className="btn btn-primary" onClick={openCreate}>
          + New Purchase Order
        </button>
      </div>

      {error && <ErrorBanner message={error} />}
      {loading && <Spinner />}
      {!loading && orders && orders.length === 0 && <EmptyState message="No purchase orders yet." />}

      {!loading && orders && orders.length > 0 && (
        <table className="data-table">
          <thead>
            <tr>
              <th>#</th>
              <th>Supplier</th>
              <th>Order date</th>
              <th>Status</th>
              <th>Items</th>
              <th />
            </tr>
          </thead>
          <tbody>
            {orders.map((o) => (
              <tr key={o.id}>
                <td>{o.id}</td>
                <td>{o.supplierName}</td>
                <td>{new Date(o.orderDateUtc).toLocaleDateString()}</td>
                <td>
                  <StatusBadge status={o.status} />
                </td>
                <td>{o.items.length}</td>
                <td className="row-actions">
                  <Link className="btn btn-small" to={`/purchase-orders/${o.id}`}>
                    View
                  </Link>
                </td>
              </tr>
            ))}
          </tbody>
        </table>
      )}

      {creating && (
        <Modal title="New Purchase Order" onClose={() => setCreating(false)} width={640}>
          <form onSubmit={handleSubmit}>
            {formError && <ErrorBanner message={formError} />}

            <label>
              Supplier
              <select required value={supplierId} onChange={(e) => setSupplierId(e.target.value)}>
                <option value="" disabled>
                  Select...
                </option>
                {suppliers?.map((s) => (
                  <option key={s.id} value={s.id}>
                    {s.name}
                  </option>
                ))}
              </select>
            </label>

            <label>
              Notes (optional)
              <textarea value={notes} onChange={(e) => setNotes(e.target.value)} />
            </label>

            <h3>Line items</h3>
            {lines.map((line, i) => (
              <div className="line-item-row" key={i}>
                <select
                  required
                  value={line.productId}
                  onChange={(e) => updateLine(i, { productId: e.target.value })}
                >
                  <option value="" disabled>
                    Product...
                  </option>
                  {products.map((p) => (
                    <option key={p.id} value={p.id}>
                      {p.sku} — {p.name}
                    </option>
                  ))}
                </select>
                <input
                  required
                  type="number"
                  min="1"
                  placeholder="Qty"
                  value={line.quantityOrdered}
                  onChange={(e) => updateLine(i, { quantityOrdered: e.target.value })}
                />
                <input
                  required
                  type="number"
                  min="0"
                  step="0.01"
                  placeholder="Unit cost"
                  value={line.unitCost}
                  onChange={(e) => updateLine(i, { unitCost: e.target.value })}
                />
                {lines.length > 1 && (
                  <button
                    type="button"
                    className="btn btn-small btn-danger"
                    onClick={() => setLines((prev) => prev.filter((_, idx) => idx !== i))}
                  >
                    Remove
                  </button>
                )}
              </div>
            ))}
            <button type="button" className="btn btn-small" onClick={() => setLines((prev) => [...prev, { ...emptyLine }])}>
              + Add line
            </button>

            <div className="form-actions">
              <button type="button" className="btn" onClick={() => setCreating(false)}>
                Cancel
              </button>
              <button type="submit" className="btn btn-primary" disabled={saving}>
                {saving ? "Creating..." : "Create"}
              </button>
            </div>
          </form>
        </Modal>
      )}
    </div>
  );
}

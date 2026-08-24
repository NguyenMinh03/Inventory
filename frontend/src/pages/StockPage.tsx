import { useState, type FormEvent } from "react";
import { stockApi, productsApi, warehousesApi } from "../api/endpoints";
import { ApiError } from "../api/client";
import { useAsync } from "../hooks/useAsync";
import { ErrorBanner, Spinner, EmptyState } from "../components/Feedback";
import type { MovementType } from "../api/types";

export function StockPage() {
  const { data: levels, loading, error, reload } = useAsync(() => stockApi.getLevels(), []);
  const { data: productsResult } = useAsync(() => productsApi.getAll({ page: 1, pageSize: 200 }), []);
  const { data: warehouses } = useAsync(() => warehousesApi.getAll(), []);

  return (
    <div>
      <div className="page-header">
        <h1>Stock</h1>
      </div>

      <div className="stock-grid">
        <div className="card">
          <h2>Current stock levels</h2>
          {error && <ErrorBanner message={error} />}
          {loading && <Spinner />}
          {!loading && levels && levels.length === 0 && <EmptyState message="No stock recorded yet." />}
          {!loading && levels && levels.length > 0 && (
            <table className="data-table">
              <thead>
                <tr>
                  <th>Product</th>
                  <th>Warehouse</th>
                  <th>Quantity</th>
                </tr>
              </thead>
              <tbody>
                {levels.map((l) => (
                  <tr key={`${l.productId}-${l.warehouseId}`}>
                    <td>
                      {l.productSku} — {l.productName}
                    </td>
                    <td>{l.warehouseName}</td>
                    <td>{l.quantityOnHand}</td>
                  </tr>
                ))}
              </tbody>
            </table>
          )}
        </div>

        <RecordMovementCard
          products={productsResult?.items ?? []}
          warehouses={warehouses ?? []}
          onRecorded={reload}
        />

        <TransferCard products={productsResult?.items ?? []} warehouses={warehouses ?? []} onRecorded={reload} />
      </div>
    </div>
  );
}

interface Option {
  id: number;
  label: string;
}

function toProductOptions(products: { id: number; sku: string; name: string }[]): Option[] {
  return products.map((p) => ({ id: p.id, label: `${p.sku} — ${p.name}` }));
}

function RecordMovementCard({
  products,
  warehouses,
  onRecorded,
}: {
  products: { id: number; sku: string; name: string }[];
  warehouses: { id: number; name: string }[];
  onRecorded: () => void;
}) {
  const [productId, setProductId] = useState("");
  const [warehouseId, setWarehouseId] = useState("");
  const [type, setType] = useState<MovementType>("In");
  const [quantity, setQuantity] = useState("1");
  const [reference, setReference] = useState("");
  const [error, setError] = useState<string | null>(null);
  const [success, setSuccess] = useState<string | null>(null);
  const [saving, setSaving] = useState(false);

  const productOptions = toProductOptions(products);

  const handleSubmit = async (e: FormEvent) => {
    e.preventDefault();
    setSaving(true);
    setError(null);
    setSuccess(null);

    try {
      await stockApi.recordMovement({
        productId: Number(productId),
        warehouseId: Number(warehouseId),
        type,
        quantity: Number(quantity),
        reference: reference || null,
      });
      setSuccess(`Recorded ${type} of ${quantity} unit(s).`);
      onRecorded();
    } catch (err) {
      setError(err instanceof ApiError ? err.message : "Failed to record movement.");
    } finally {
      setSaving(false);
    }
  };

  return (
    <div className="card">
      <h2>Record a movement</h2>
      <form onSubmit={handleSubmit}>
        {error && <ErrorBanner message={error} />}
        {success && <div className="banner banner-success">{success}</div>}

        <label>
          Product
          <select required value={productId} onChange={(e) => setProductId(e.target.value)}>
            <option value="" disabled>
              Select...
            </option>
            {productOptions.map((o) => (
              <option key={o.id} value={o.id}>
                {o.label}
              </option>
            ))}
          </select>
        </label>

        <label>
          Warehouse
          <select required value={warehouseId} onChange={(e) => setWarehouseId(e.target.value)}>
            <option value="" disabled>
              Select...
            </option>
            {warehouses.map((w) => (
              <option key={w.id} value={w.id}>
                {w.name}
              </option>
            ))}
          </select>
        </label>

        <div className="form-row">
          <label>
            Type
            <select value={type} onChange={(e) => setType(e.target.value as MovementType)}>
              <option value="In">In</option>
              <option value="Out">Out</option>
              <option value="Adjustment">Adjustment</option>
            </select>
          </label>

          <label>
            Quantity
            <input
              required
              type="number"
              min="1"
              value={quantity}
              onChange={(e) => setQuantity(e.target.value)}
            />
          </label>
        </div>

        <label>
          Reference (optional)
          <input value={reference} onChange={(e) => setReference(e.target.value)} />
        </label>

        <div className="form-actions">
          <button type="submit" className="btn btn-primary" disabled={saving}>
            {saving ? "Recording..." : "Record movement"}
          </button>
        </div>
      </form>
    </div>
  );
}

function TransferCard({
  products,
  warehouses,
  onRecorded,
}: {
  products: { id: number; sku: string; name: string }[];
  warehouses: { id: number; name: string }[];
  onRecorded: () => void;
}) {
  const [productId, setProductId] = useState("");
  const [sourceWarehouseId, setSourceWarehouseId] = useState("");
  const [destinationWarehouseId, setDestinationWarehouseId] = useState("");
  const [quantity, setQuantity] = useState("1");
  const [error, setError] = useState<string | null>(null);
  const [success, setSuccess] = useState<string | null>(null);
  const [saving, setSaving] = useState(false);

  const productOptions = toProductOptions(products);

  const handleSubmit = async (e: FormEvent) => {
    e.preventDefault();
    setSaving(true);
    setError(null);
    setSuccess(null);

    try {
      await stockApi.transfer({
        productId: Number(productId),
        sourceWarehouseId: Number(sourceWarehouseId),
        destinationWarehouseId: Number(destinationWarehouseId),
        quantity: Number(quantity),
      });
      setSuccess(`Transferred ${quantity} unit(s).`);
      onRecorded();
    } catch (err) {
      setError(err instanceof ApiError ? err.message : "Failed to transfer stock.");
    } finally {
      setSaving(false);
    }
  };

  return (
    <div className="card">
      <h2>Transfer between warehouses</h2>
      <form onSubmit={handleSubmit}>
        {error && <ErrorBanner message={error} />}
        {success && <div className="banner banner-success">{success}</div>}

        <label>
          Product
          <select required value={productId} onChange={(e) => setProductId(e.target.value)}>
            <option value="" disabled>
              Select...
            </option>
            {productOptions.map((o) => (
              <option key={o.id} value={o.id}>
                {o.label}
              </option>
            ))}
          </select>
        </label>

        <div className="form-row">
          <label>
            From warehouse
            <select required value={sourceWarehouseId} onChange={(e) => setSourceWarehouseId(e.target.value)}>
              <option value="" disabled>
                Select...
              </option>
              {warehouses.map((w) => (
                <option key={w.id} value={w.id}>
                  {w.name}
                </option>
              ))}
            </select>
          </label>

          <label>
            To warehouse
            <select
              required
              value={destinationWarehouseId}
              onChange={(e) => setDestinationWarehouseId(e.target.value)}
            >
              <option value="" disabled>
                Select...
              </option>
              {warehouses.map((w) => (
                <option key={w.id} value={w.id}>
                  {w.name}
                </option>
              ))}
            </select>
          </label>
        </div>

        <label>
          Quantity
          <input required type="number" min="1" value={quantity} onChange={(e) => setQuantity(e.target.value)} />
        </label>

        <div className="form-actions">
          <button type="submit" className="btn btn-primary" disabled={saving}>
            {saving ? "Transferring..." : "Transfer"}
          </button>
        </div>
      </form>
    </div>
  );
}

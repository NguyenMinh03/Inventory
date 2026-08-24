import { useState, type FormEvent } from "react";
import { productsApi, categoriesApi } from "../api/endpoints";
import { ApiError } from "../api/client";
import { useAsync } from "../hooks/useAsync";
import { useAuth } from "../auth/AuthContext";
import { Modal } from "../components/Modal";
import { ErrorBanner, Spinner, EmptyState } from "../components/Feedback";
import type { Product } from "../api/types";

interface FormState {
  sku: string;
  name: string;
  description: string;
  unitOfMeasure: string;
  unitPrice: string;
  reorderLevel: string;
  categoryId: string;
  isActive: boolean;
}

const emptyForm: FormState = {
  sku: "",
  name: "",
  description: "",
  unitOfMeasure: "each",
  unitPrice: "0",
  reorderLevel: "0",
  categoryId: "",
  isActive: true,
};

const SORT_OPTIONS = [
  { value: "", label: "Name (A-Z)" },
  { value: "name_desc", label: "Name (Z-A)" },
  { value: "sku", label: "SKU (A-Z)" },
  { value: "price", label: "Price (low-high)" },
  { value: "price_desc", label: "Price (high-low)" },
  { value: "reorderlevel_desc", label: "Reorder level (high-low)" },
];

export function ProductsPage() {
  const { canDelete } = useAuth();
  const [page, setPage] = useState(1);
  const [search, setSearch] = useState("");
  const [sortBy, setSortBy] = useState("");

  const {
    data: result,
    loading,
    error,
    reload,
  } = useAsync(() => productsApi.getAll({ page, pageSize: 10, search, sortBy: sortBy || undefined }), [
    page,
    search,
    sortBy,
  ]);

  const { data: categories } = useAsync(() => categoriesApi.getAll(), []);

  const [editing, setEditing] = useState<Product | null>(null);
  const [creating, setCreating] = useState(false);
  const [form, setForm] = useState<FormState>(emptyForm);
  const [formError, setFormError] = useState<string | null>(null);
  const [saving, setSaving] = useState(false);

  const openCreate = () => {
    setForm({ ...emptyForm, categoryId: categories?.[0]?.id.toString() ?? "" });
    setFormError(null);
    setCreating(true);
  };

  const openEdit = (product: Product) => {
    setEditing(product);
    setForm({
      sku: product.sku,
      name: product.name,
      description: product.description ?? "",
      unitOfMeasure: product.unitOfMeasure,
      unitPrice: product.unitPrice.toString(),
      reorderLevel: product.reorderLevel.toString(),
      categoryId: product.categoryId.toString(),
      isActive: product.isActive,
    });
    setFormError(null);
  };

  const closeForm = () => {
    setCreating(false);
    setEditing(null);
  };

  const handleSubmit = async (e: FormEvent) => {
    e.preventDefault();
    setSaving(true);
    setFormError(null);

    try {
      if (editing) {
        await productsApi.update(editing.id, {
          name: form.name,
          description: form.description || null,
          unitOfMeasure: form.unitOfMeasure,
          unitPrice: Number(form.unitPrice),
          reorderLevel: Number(form.reorderLevel),
          categoryId: Number(form.categoryId),
          isActive: form.isActive,
        });
      } else {
        await productsApi.create({
          sku: form.sku,
          name: form.name,
          description: form.description || null,
          unitOfMeasure: form.unitOfMeasure,
          unitPrice: Number(form.unitPrice),
          reorderLevel: Number(form.reorderLevel),
          categoryId: Number(form.categoryId),
        });
      }
      closeForm();
      reload();
    } catch (err) {
      setFormError(err instanceof ApiError ? err.message : "Failed to save product.");
    } finally {
      setSaving(false);
    }
  };

  const handleDelete = async (product: Product) => {
    if (!confirm(`Delete product "${product.name}"?`)) return;
    try {
      await productsApi.remove(product.id);
      reload();
    } catch (err) {
      alert(err instanceof ApiError ? err.message : "Failed to delete product.");
    }
  };

  const showForm = creating || editing !== null;

  return (
    <div>
      <div className="page-header">
        <h1>Products</h1>
        <button className="btn btn-primary" onClick={openCreate}>
          + New Product
        </button>
      </div>

      <div className="toolbar">
        <input
          className="search-input"
          placeholder="Search by name or SKU..."
          value={search}
          onChange={(e) => {
            setSearch(e.target.value);
            setPage(1);
          }}
        />
        <select
          value={sortBy}
          onChange={(e) => {
            setSortBy(e.target.value);
            setPage(1);
          }}
        >
          {SORT_OPTIONS.map((o) => (
            <option key={o.value} value={o.value}>
              {o.label}
            </option>
          ))}
        </select>
      </div>

      {error && <ErrorBanner message={error} />}
      {loading && <Spinner />}

      {!loading && result && result.items.length === 0 && <EmptyState message="No products match." />}

      {!loading && result && result.items.length > 0 && (
        <>
          <table className="data-table">
            <thead>
              <tr>
                <th>SKU</th>
                <th>Name</th>
                <th>Category</th>
                <th>Price</th>
                <th>Reorder level</th>
                <th>Status</th>
                <th />
              </tr>
            </thead>
            <tbody>
              {result.items.map((p) => (
                <tr key={p.id}>
                  <td>{p.sku}</td>
                  <td>{p.name}</td>
                  <td>{p.categoryName ?? "—"}</td>
                  <td>${p.unitPrice.toFixed(2)}</td>
                  <td>{p.reorderLevel}</td>
                  <td>
                    <span className={"status-badge " + (p.isActive ? "status-active" : "status-inactive")}>
                      {p.isActive ? "Active" : "Inactive"}
                    </span>
                  </td>
                  <td className="row-actions">
                    <button className="btn btn-small" onClick={() => openEdit(p)}>
                      Edit
                    </button>
                    {canDelete && (
                      <button className="btn btn-small btn-danger" onClick={() => handleDelete(p)}>
                        Delete
                      </button>
                    )}
                  </td>
                </tr>
              ))}
            </tbody>
          </table>

          <div className="pagination">
            <button className="btn btn-small" disabled={page <= 1} onClick={() => setPage((p) => p - 1)}>
              Previous
            </button>
            <span>
              Page {result.page} of {result.totalPages} ({result.totalCount} total)
            </span>
            <button
              className="btn btn-small"
              disabled={page >= result.totalPages}
              onClick={() => setPage((p) => p + 1)}
            >
              Next
            </button>
          </div>
        </>
      )}

      {showForm && (
        <Modal title={editing ? "Edit Product" : "New Product"} onClose={closeForm} width={520}>
          <form onSubmit={handleSubmit}>
            {formError && <ErrorBanner message={formError} />}

            {!editing && (
              <label>
                SKU
                <input required value={form.sku} onChange={(e) => setForm({ ...form, sku: e.target.value })} />
              </label>
            )}

            <label>
              Name
              <input required value={form.name} onChange={(e) => setForm({ ...form, name: e.target.value })} />
            </label>

            <label>
              Description
              <textarea
                value={form.description}
                onChange={(e) => setForm({ ...form, description: e.target.value })}
              />
            </label>

            <div className="form-row">
              <label>
                Unit of measure
                <input
                  required
                  value={form.unitOfMeasure}
                  onChange={(e) => setForm({ ...form, unitOfMeasure: e.target.value })}
                />
              </label>

              <label>
                Unit price
                <input
                  required
                  type="number"
                  step="0.01"
                  min="0"
                  value={form.unitPrice}
                  onChange={(e) => setForm({ ...form, unitPrice: e.target.value })}
                />
              </label>
            </div>

            <div className="form-row">
              <label>
                Reorder level
                <input
                  required
                  type="number"
                  min="0"
                  value={form.reorderLevel}
                  onChange={(e) => setForm({ ...form, reorderLevel: e.target.value })}
                />
              </label>

              <label>
                Category
                <select
                  required
                  value={form.categoryId}
                  onChange={(e) => setForm({ ...form, categoryId: e.target.value })}
                >
                  <option value="" disabled>
                    Select...
                  </option>
                  {categories?.map((c) => (
                    <option key={c.id} value={c.id}>
                      {c.name}
                    </option>
                  ))}
                </select>
              </label>
            </div>

            {editing && (
              <label className="checkbox-label">
                <input
                  type="checkbox"
                  checked={form.isActive}
                  onChange={(e) => setForm({ ...form, isActive: e.target.checked })}
                />
                Active
              </label>
            )}

            <div className="form-actions">
              <button type="button" className="btn" onClick={closeForm}>
                Cancel
              </button>
              <button type="submit" className="btn btn-primary" disabled={saving}>
                {saving ? "Saving..." : "Save"}
              </button>
            </div>
          </form>
        </Modal>
      )}
    </div>
  );
}

import { useState, type FormEvent } from "react";
import { warehousesApi } from "../api/endpoints";
import { ApiError } from "../api/client";
import { useAsync } from "../hooks/useAsync";
import { useAuth } from "../auth/AuthContext";
import { Modal } from "../components/Modal";
import { ErrorBanner, Spinner, EmptyState } from "../components/Feedback";
import type { Warehouse } from "../api/types";

interface FormState {
  name: string;
  address: string;
  isActive: boolean;
}

const emptyForm: FormState = { name: "", address: "", isActive: true };

export function WarehousesPage() {
  const { canDelete } = useAuth();
  const { data: warehouses, loading, error, reload } = useAsync(() => warehousesApi.getAll(), []);

  const [editing, setEditing] = useState<Warehouse | null>(null);
  const [creating, setCreating] = useState(false);
  const [form, setForm] = useState<FormState>(emptyForm);
  const [formError, setFormError] = useState<string | null>(null);
  const [saving, setSaving] = useState(false);

  const openCreate = () => {
    setForm(emptyForm);
    setFormError(null);
    setCreating(true);
  };

  const openEdit = (warehouse: Warehouse) => {
    setEditing(warehouse);
    setForm({ name: warehouse.name, address: warehouse.address ?? "", isActive: warehouse.isActive });
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
        await warehousesApi.update(editing.id, {
          name: form.name,
          address: form.address || null,
          isActive: form.isActive,
        });
      } else {
        await warehousesApi.create({ name: form.name, address: form.address || null });
      }
      closeForm();
      reload();
    } catch (err) {
      setFormError(err instanceof ApiError ? err.message : "Failed to save warehouse.");
    } finally {
      setSaving(false);
    }
  };

  const handleDelete = async (warehouse: Warehouse) => {
    if (!confirm(`Delete warehouse "${warehouse.name}"?`)) return;
    try {
      await warehousesApi.remove(warehouse.id);
      reload();
    } catch (err) {
      alert(err instanceof ApiError ? err.message : "Failed to delete warehouse.");
    }
  };

  const showForm = creating || editing !== null;

  return (
    <div>
      <div className="page-header">
        <h1>Warehouses</h1>
        <button className="btn btn-primary" onClick={openCreate}>
          + New Warehouse
        </button>
      </div>

      {error && <ErrorBanner message={error} />}
      {loading && <Spinner />}
      {!loading && warehouses && warehouses.length === 0 && <EmptyState message="No warehouses yet." />}

      {!loading && warehouses && warehouses.length > 0 && (
        <table className="data-table">
          <thead>
            <tr>
              <th>Name</th>
              <th>Address</th>
              <th>Status</th>
              <th />
            </tr>
          </thead>
          <tbody>
            {warehouses.map((w) => (
              <tr key={w.id}>
                <td>{w.name}</td>
                <td>{w.address ?? "—"}</td>
                <td>
                  <span className={"status-badge " + (w.isActive ? "status-active" : "status-inactive")}>
                    {w.isActive ? "Active" : "Inactive"}
                  </span>
                </td>
                <td className="row-actions">
                  <button className="btn btn-small" onClick={() => openEdit(w)}>
                    Edit
                  </button>
                  {canDelete && (
                    <button className="btn btn-small btn-danger" onClick={() => handleDelete(w)}>
                      Delete
                    </button>
                  )}
                </td>
              </tr>
            ))}
          </tbody>
        </table>
      )}

      {showForm && (
        <Modal title={editing ? "Edit Warehouse" : "New Warehouse"} onClose={closeForm}>
          <form onSubmit={handleSubmit}>
            {formError && <ErrorBanner message={formError} />}

            <label>
              Name
              <input required value={form.name} onChange={(e) => setForm({ ...form, name: e.target.value })} />
            </label>

            <label>
              Address
              <input value={form.address} onChange={(e) => setForm({ ...form, address: e.target.value })} />
            </label>

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

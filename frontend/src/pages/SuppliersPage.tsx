import { useState, type FormEvent } from "react";
import { suppliersApi } from "../api/endpoints";
import { ApiError } from "../api/client";
import { useAsync } from "../hooks/useAsync";
import { useAuth } from "../auth/AuthContext";
import { Modal } from "../components/Modal";
import { ErrorBanner, Spinner, EmptyState } from "../components/Feedback";
import type { Supplier } from "../api/types";

interface FormState {
  name: string;
  contactName: string;
  email: string;
  phone: string;
  address: string;
  isActive: boolean;
}

const emptyForm: FormState = { name: "", contactName: "", email: "", phone: "", address: "", isActive: true };

export function SuppliersPage() {
  const { canDelete } = useAuth();
  const { data: suppliers, loading, error, reload } = useAsync(() => suppliersApi.getAll(), []);

  const [editing, setEditing] = useState<Supplier | null>(null);
  const [creating, setCreating] = useState(false);
  const [form, setForm] = useState<FormState>(emptyForm);
  const [formError, setFormError] = useState<string | null>(null);
  const [saving, setSaving] = useState(false);

  const openCreate = () => {
    setForm(emptyForm);
    setFormError(null);
    setCreating(true);
  };

  const openEdit = (supplier: Supplier) => {
    setEditing(supplier);
    setForm({
      name: supplier.name,
      contactName: supplier.contactName ?? "",
      email: supplier.email ?? "",
      phone: supplier.phone ?? "",
      address: supplier.address ?? "",
      isActive: supplier.isActive,
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

    const base = {
      name: form.name,
      contactName: form.contactName || null,
      email: form.email || null,
      phone: form.phone || null,
      address: form.address || null,
    };

    try {
      if (editing) {
        await suppliersApi.update(editing.id, { ...base, isActive: form.isActive });
      } else {
        await suppliersApi.create(base);
      }
      closeForm();
      reload();
    } catch (err) {
      setFormError(err instanceof ApiError ? err.message : "Failed to save supplier.");
    } finally {
      setSaving(false);
    }
  };

  const handleDelete = async (supplier: Supplier) => {
    if (!confirm(`Delete supplier "${supplier.name}"?`)) return;
    try {
      await suppliersApi.remove(supplier.id);
      reload();
    } catch (err) {
      alert(err instanceof ApiError ? err.message : "Failed to delete supplier.");
    }
  };

  const showForm = creating || editing !== null;

  return (
    <div>
      <div className="page-header">
        <h1>Suppliers</h1>
        <button className="btn btn-primary" onClick={openCreate}>
          + New Supplier
        </button>
      </div>

      {error && <ErrorBanner message={error} />}
      {loading && <Spinner />}
      {!loading && suppliers && suppliers.length === 0 && <EmptyState message="No suppliers yet." />}

      {!loading && suppliers && suppliers.length > 0 && (
        <table className="data-table">
          <thead>
            <tr>
              <th>Name</th>
              <th>Contact</th>
              <th>Email</th>
              <th>Status</th>
              <th />
            </tr>
          </thead>
          <tbody>
            {suppliers.map((s) => (
              <tr key={s.id}>
                <td>{s.name}</td>
                <td>{s.contactName ?? "—"}</td>
                <td>{s.email ?? "—"}</td>
                <td>
                  <span className={"status-badge " + (s.isActive ? "status-active" : "status-inactive")}>
                    {s.isActive ? "Active" : "Inactive"}
                  </span>
                </td>
                <td className="row-actions">
                  <button className="btn btn-small" onClick={() => openEdit(s)}>
                    Edit
                  </button>
                  {canDelete && (
                    <button className="btn btn-small btn-danger" onClick={() => handleDelete(s)}>
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
        <Modal title={editing ? "Edit Supplier" : "New Supplier"} onClose={closeForm} width={520}>
          <form onSubmit={handleSubmit}>
            {formError && <ErrorBanner message={formError} />}

            <label>
              Name
              <input required value={form.name} onChange={(e) => setForm({ ...form, name: e.target.value })} />
            </label>

            <label>
              Contact name
              <input value={form.contactName} onChange={(e) => setForm({ ...form, contactName: e.target.value })} />
            </label>

            <label>
              Email
              <input
                type="email"
                value={form.email}
                onChange={(e) => setForm({ ...form, email: e.target.value })}
              />
            </label>

            <label>
              Phone
              <input value={form.phone} onChange={(e) => setForm({ ...form, phone: e.target.value })} />
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

import { useState, type FormEvent } from "react";
import { categoriesApi } from "../api/endpoints";
import { ApiError } from "../api/client";
import { useAsync } from "../hooks/useAsync";
import { useAuth } from "../auth/AuthContext";
import { Modal } from "../components/Modal";
import { ErrorBanner, Spinner, EmptyState } from "../components/Feedback";
import type { Category } from "../api/types";

interface FormState {
  name: string;
  description: string;
  parentCategoryId: string;
}

const emptyForm: FormState = { name: "", description: "", parentCategoryId: "" };

export function CategoriesPage() {
  const { canDelete } = useAuth();
  const { data: categories, loading, error, reload } = useAsync(() => categoriesApi.getAll(), []);

  const [editing, setEditing] = useState<Category | null>(null);
  const [creating, setCreating] = useState(false);
  const [form, setForm] = useState<FormState>(emptyForm);
  const [formError, setFormError] = useState<string | null>(null);
  const [saving, setSaving] = useState(false);

  const openCreate = () => {
    setForm(emptyForm);
    setFormError(null);
    setCreating(true);
  };

  const openEdit = (category: Category) => {
    setEditing(category);
    setForm({
      name: category.name,
      description: category.description ?? "",
      parentCategoryId: category.parentCategoryId?.toString() ?? "",
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

    const body = {
      name: form.name,
      description: form.description || null,
      parentCategoryId: form.parentCategoryId ? Number(form.parentCategoryId) : null,
    };

    try {
      if (editing) {
        await categoriesApi.update(editing.id, body);
      } else {
        await categoriesApi.create(body);
      }
      closeForm();
      reload();
    } catch (err) {
      setFormError(err instanceof ApiError ? err.message : "Failed to save category.");
    } finally {
      setSaving(false);
    }
  };

  const handleDelete = async (category: Category) => {
    if (!confirm(`Delete category "${category.name}"?`)) return;
    try {
      await categoriesApi.remove(category.id);
      reload();
    } catch (err) {
      alert(err instanceof ApiError ? err.message : "Failed to delete category.");
    }
  };

  const showForm = creating || editing !== null;

  return (
    <div>
      <div className="page-header">
        <h1>Categories</h1>
        <button className="btn btn-primary" onClick={openCreate}>
          + New Category
        </button>
      </div>

      {error && <ErrorBanner message={error} />}
      {loading && <Spinner />}

      {!loading && categories && categories.length === 0 && <EmptyState message="No categories yet." />}

      {!loading && categories && categories.length > 0 && (
        <table className="data-table">
          <thead>
            <tr>
              <th>Name</th>
              <th>Description</th>
              <th>Parent</th>
              <th />
            </tr>
          </thead>
          <tbody>
            {categories.map((c) => (
              <tr key={c.id}>
                <td>{c.name}</td>
                <td>{c.description ?? "—"}</td>
                <td>{c.parentCategoryName ?? "—"}</td>
                <td className="row-actions">
                  <button className="btn btn-small" onClick={() => openEdit(c)}>
                    Edit
                  </button>
                  {canDelete && (
                    <button className="btn btn-small btn-danger" onClick={() => handleDelete(c)}>
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
        <Modal title={editing ? "Edit Category" : "New Category"} onClose={closeForm}>
          <form onSubmit={handleSubmit}>
            {formError && <ErrorBanner message={formError} />}

            <label>
              Name
              <input
                required
                value={form.name}
                onChange={(e) => setForm({ ...form, name: e.target.value })}
              />
            </label>

            <label>
              Description
              <textarea
                value={form.description}
                onChange={(e) => setForm({ ...form, description: e.target.value })}
              />
            </label>

            <label>
              Parent category ID (optional)
              <input
                type="number"
                value={form.parentCategoryId}
                onChange={(e) => setForm({ ...form, parentCategoryId: e.target.value })}
              />
            </label>

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

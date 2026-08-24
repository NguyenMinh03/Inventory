import { useState, type FormEvent } from "react";
import { useLocation, useNavigate } from "react-router-dom";
import { useAuth } from "../auth/AuthContext";
import { ApiError } from "../api/client";
import { ErrorBanner } from "../components/Feedback";

const DEMO_ACCOUNTS = [
  { username: "admin", password: "Admin123!", role: "Admin" },
  { username: "manager", password: "Manager123!", role: "Manager" },
  { username: "staff", password: "Staff123!", role: "Staff" },
];

export function LoginPage() {
  const { login } = useAuth();
  const navigate = useNavigate();
  const location = useLocation();
  const from = (location.state as { from?: Location })?.from?.pathname ?? "/";

  const [username, setUsername] = useState("");
  const [password, setPassword] = useState("");
  const [error, setError] = useState<string | null>(null);
  const [submitting, setSubmitting] = useState(false);

  const handleSubmit = async (e: FormEvent) => {
    e.preventDefault();
    setSubmitting(true);
    setError(null);
    try {
      await login(username, password);
      navigate(from, { replace: true });
    } catch (err) {
      setError(err instanceof ApiError ? err.message : "Login failed.");
    } finally {
      setSubmitting(false);
    }
  };

  return (
    <div className="login-page">
      <form className="login-card" onSubmit={handleSubmit}>
        <h1>Inventory System</h1>
        <p className="login-subtitle">Sign in to continue</p>

        {error && <ErrorBanner message={error} />}

        <label>
          Username
          <input required autoFocus value={username} onChange={(e) => setUsername(e.target.value)} />
        </label>

        <label>
          Password
          <input required type="password" value={password} onChange={(e) => setPassword(e.target.value)} />
        </label>

        <button type="submit" className="btn btn-primary btn-block" disabled={submitting}>
          {submitting ? "Signing in..." : "Sign in"}
        </button>

        <div className="demo-accounts">
          <p>Demo accounts:</p>
          {DEMO_ACCOUNTS.map((acc) => (
            <button
              type="button"
              key={acc.username}
              className="demo-account-btn"
              onClick={() => {
                setUsername(acc.username);
                setPassword(acc.password);
              }}
            >
              <strong>{acc.username}</strong> ({acc.role})
            </button>
          ))}
        </div>
      </form>
    </div>
  );
}

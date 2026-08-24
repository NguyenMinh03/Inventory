import { createContext, useContext, useEffect, useMemo, useState, type ReactNode } from "react";
import { authApi } from "../api/endpoints";
import { setAuthToken } from "../api/client";
import type { AuthResult, UserRole } from "../api/types";

const STORAGE_KEY = "inventory.auth";

interface AuthState {
  token: string;
  username: string;
  role: UserRole;
  expiresAtUtc: string;
}

interface AuthContextValue {
  user: AuthState | null;
  isAuthenticated: boolean;
  canDelete: boolean;
  login: (username: string, password: string) => Promise<void>;
  logout: () => void;
}

const AuthContext = createContext<AuthContextValue | undefined>(undefined);

function loadStoredAuth(): AuthState | null {
  const raw = localStorage.getItem(STORAGE_KEY);
  if (!raw) return null;
  try {
    const parsed = JSON.parse(raw) as AuthState;
    if (new Date(parsed.expiresAtUtc).getTime() <= Date.now()) {
      localStorage.removeItem(STORAGE_KEY);
      return null;
    }
    return parsed;
  } catch {
    return null;
  }
}

export function AuthProvider({ children }: { children: ReactNode }) {
  const [user, setUser] = useState<AuthState | null>(() => loadStoredAuth());

  useEffect(() => {
    setAuthToken(user?.token ?? null);
  }, [user]);

  const login = async (username: string, password: string) => {
    const result: AuthResult = await authApi.login({ username, password });
    const state: AuthState = {
      token: result.token,
      username: result.username,
      role: result.role,
      expiresAtUtc: result.expiresAtUtc,
    };
    localStorage.setItem(STORAGE_KEY, JSON.stringify(state));
    setUser(state);
  };

  const logout = () => {
    localStorage.removeItem(STORAGE_KEY);
    setUser(null);
  };

  const value = useMemo<AuthContextValue>(
    () => ({
      user,
      isAuthenticated: user !== null,
      // Matches [Authorize(Roles = "Admin,Manager")] on the delete endpoints -
      // purely a UI convenience (hide/disable the button); the API enforces
      // this for real regardless of what the client sends.
      canDelete: user?.role === "Admin" || user?.role === "Manager",
      login,
      logout,
    }),
    [user],
  );

  return <AuthContext.Provider value={value}>{children}</AuthContext.Provider>;
}

export function useAuth(): AuthContextValue {
  const ctx = useContext(AuthContext);
  if (!ctx) throw new Error("useAuth must be used within an AuthProvider");
  return ctx;
}

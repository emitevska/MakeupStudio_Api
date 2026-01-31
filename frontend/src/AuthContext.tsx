import { createContext, useContext, useState, useEffect } from 'react';
import type { ReactNode } from 'react';
import { setAuthToken } from './api';

interface AuthContextType {
  isAuthenticated: boolean;
  userEmail: string | null;
  isAdmin: boolean;
  login: (token: string) => void;
  logout: () => void;
}

const AuthContext = createContext<AuthContextType | undefined>(undefined);

function parseJwt(token: string) {
  try {
    const base64Url = token.split('.')[1];
    const base64 = base64Url.replace(/-/g, '+').replace(/_/g, '/');
    const jsonPayload = decodeURIComponent(
      atob(base64)
        .split('')
        .map((c) => '%' + ('00' + c.charCodeAt(0).toString(16)).slice(-2))
        .join('')
    );
    return JSON.parse(jsonPayload);
  } catch {
    return null;
  }
}

export function AuthProvider({ children }: { children: ReactNode }) {
  const [isAuthenticated, setIsAuthenticated] = useState(false);
  const [userEmail, setUserEmail] = useState<string | null>(null);
  const [isAdmin, setIsAdmin] = useState(false);

  useEffect(() => {
    const token = localStorage.getItem('token');
    if (token) {
      const payload = parseJwt(token);
      if (payload) {
        setIsAuthenticated(true);
        setUserEmail(payload.email || payload.sub);
        setIsAdmin(
          payload.role === 'Admin' ||
          (Array.isArray(payload.role) && payload.role.includes('Admin')) ||
          payload['http://schemas.microsoft.com/ws/2008/06/identity/claims/role'] === 'Admin'
        );
        setAuthToken(token);
      }
    }
  }, []);

  const login = (token: string) => {
    const payload = parseJwt(token);
    if (payload) {
      setIsAuthenticated(true);
      setUserEmail(payload.email || payload.sub);
      setIsAdmin(
        payload.role === 'Admin' ||
        (Array.isArray(payload.role) && payload.role.includes('Admin')) ||
        payload['http://schemas.microsoft.com/ws/2008/06/identity/claims/role'] === 'Admin'
      );
      setAuthToken(token);
    }
  };

  const logout = () => {
    setIsAuthenticated(false);
    setUserEmail(null);
    setIsAdmin(false);
    setAuthToken(null);
  };

  return (
    <AuthContext.Provider value={{ isAuthenticated, userEmail, isAdmin, login, logout }}>
      {children}
    </AuthContext.Provider>
  );
}

export function useAuth() {
  const context = useContext(AuthContext);
  if (!context) {
    throw new Error('useAuth must be used within AuthProvider');
  }
  return context;
}

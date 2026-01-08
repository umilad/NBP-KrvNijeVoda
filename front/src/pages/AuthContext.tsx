import { createContext, useContext, useState, useEffect } from "react";
import type { ReactNode } from "react"; // <--- ovako
import axios from "axios";
import { useNavigate } from "react-router-dom";


interface AuthContextType {
  username: string | null;
  token: string | null;
  role: string | null;
  login: (username: string, token: string, role: string) => void;
  logout: (sessionExpired?: boolean) => void;
}

const AuthContext = createContext<AuthContextType | undefined>(undefined);

// ✅ Helper funkcija: proverava da li JWT još važi
function isTokenValid(token: string | null): boolean {
  if (!token) return false;
  try {
    const payload = JSON.parse(atob(token.split(".")[1]));
    const now = Math.floor(Date.now() / 1000);
    return payload.exp > now;
  } catch {
    return false;
  }
}

// ✅ Helper: koliko milisekundi do isteka tokena
function getTokenExpirationDelay(token: string): number {
  try {
    const payload = JSON.parse(atob(token.split(".")[1]));
    const now = Math.floor(Date.now() / 1000);
    return (payload.exp - now) * 1000; // u ms
  } catch {
    return 0;
  }
}

export const AuthProvider = ({ children }: { children: ReactNode }) => {
  const navigate = useNavigate();
  const [username, setUsername] = useState<string | null>(null);
  const [token, setToken] = useState<string | null>(null);
  const [role, setRole] = useState<string | null>(null);
  const [logoutTimeout, setLogoutTimeout] = useState<number | null>(null);


  // Logout funkcija
  const logout = async (sessionExpired: boolean = false) => {
    if (token) {
      try {
        await axios.post(
          "http://localhost:5210/api/Auth/logout",
          {},
          { headers: { Authorization: `Bearer ${token}` } }
        );
      } catch (err) {
        console.error("Logout error", err);
      }
    }

    setUsername(null);
    setToken(null);
    setRole(null);
    localStorage.removeItem("token");
    localStorage.removeItem("username");
    localStorage.removeItem("role");

    if (logoutTimeout) {
      clearTimeout(logoutTimeout);
      setLogoutTimeout(null);
    }

    if (sessionExpired) {
      alert("Sesija je istekla, ulogujte se opet.");
    }
    navigate("/prijava");
  };

  // Login funkcija
  const login = (username: string, token: string, role: string) => {
    setUsername(username);
    setToken(token);
    setRole(role);

    localStorage.setItem("token", token);
    localStorage.setItem("username", username);
    localStorage.setItem("role", role);

    // 🕒 automatski logout kad token istekne
    const delay = getTokenExpirationDelay(token);
    if (delay > 0) {
      const timeout = setTimeout(() => logout(true), delay);
      setLogoutTimeout(timeout);
    }
  };

  // Auto-login pri mount-u
  useEffect(() => {
    const storedToken = localStorage.getItem("token");
    const storedUsername = localStorage.getItem("username");
    const storedRole = localStorage.getItem("role");

    if (storedToken && storedUsername && isTokenValid(storedToken)) {
      setToken(storedToken);
      setUsername(storedUsername);
      setRole(storedRole);

      // 🕒 automatski logout kad token istekne
      const delay = getTokenExpirationDelay(storedToken);
      if (delay > 0) {
        const timeout = setTimeout(() => logout(true), delay);
        setLogoutTimeout(timeout);
      }
    }
    // Ako nema tokena ili je istekao -> ne pokazuje alert odmah
  }, []);

  // Axios interceptor za 401 (token istekao / nevalidan)
  useEffect(() => {
    const interceptor = axios.interceptors.response.use(
      response => response,
      error => {
        if (error.response?.status === 401) {
          logout(true);
        }
        return Promise.reject(error);
      }
    );

    return () => {
      axios.interceptors.response.eject(interceptor);
    };
  }, [token]);

  return (
    <AuthContext.Provider value={{ username, token, role, login, logout }}>
      {children}
    </AuthContext.Provider>
  );
};

export const useAuth = () => {
  const context = useContext(AuthContext);
  if (!context) throw new Error("useAuth must be used within AuthProvider");
  return context;
};

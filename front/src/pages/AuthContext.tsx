import { createContext, useContext, useState, ReactNode, useEffect } from "react";
import axios from "axios";

interface AuthContextType {
  username: string | null;
  token: string | null;
  role: string | null;
  login: (username: string, token: string, role: string) => void;
  logout: () => void;
}

const AuthContext = createContext<AuthContextType | undefined>(undefined);

export const AuthProvider = ({ children }: { children: ReactNode }) => {
  const [username, setUsername] = useState<string | null>(null);
  const [token, setToken] = useState<string | null>(null);
  const [role, setRole] = useState<string | null>(null);

  useEffect(() => {
    const storedToken = localStorage.getItem("token");
    const storedUsername = localStorage.getItem("username");
    const storedRole = localStorage.getItem("role");
    if (storedToken && storedUsername) {
      setToken(storedToken);
      setUsername(storedUsername);
      setRole(storedRole);
    }
  }, []);

  const login = (username: string, token: string, role: string) => {
    setUsername(username);
    setToken(token);
    setRole(role);
    localStorage.setItem("token", token);
    localStorage.setItem("username", username);
    localStorage.setItem("role", role);
  };

  const logout = async () => {
    if (token) {
      try {
        await axios.post("http://localhost:5210/api/Auth/logout", {}, {
          headers: { Authorization: `Bearer ${token}` }
        });
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
  };

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

import React, { createContext, useContext, useState, useEffect } from 'react';
import { UsuarioSession } from '../types';

interface AuthContextType {
  user: UsuarioSession | null;
  token: string | null;
  login: (token: string, user: UsuarioSession) => void;
  logout: () => void;
  isAuthenticated: boolean;
  isAdmin: boolean;
  isColaborador: boolean;
  isCliente: boolean;
}

const AuthContext = createContext<AuthContextType | undefined>(undefined);

export const AuthProvider: React.FC<{ children: React.ReactNode }> = ({ children }) => {
  const [token, setToken] = useState<string | null>(() => localStorage.getItem('token'));
  const [user, setUser] = useState<UsuarioSession | null>(() => {
    const saved = localStorage.getItem('user');
    return saved ? JSON.parse(saved) : null;
  });

  const login = (newToken: string, newUser: UsuarioSession) => {
    localStorage.setItem('token', newToken);
    localStorage.setItem('user', JSON.stringify(newUser));
    setToken(newToken);
    setUser(newUser);
  };

  const logout = () => {
    localStorage.removeItem('token');
    localStorage.removeItem('user');
    setToken(null);
    setUser(null);
    window.location.href = '/login';
  };

  const isAuthenticated = !!token && !!user;
  const isAdmin = user?.rol === 'AdminEstudio';
  const isColaborador = user?.rol === 'Colaborador';
  const isCliente = user?.rol === 'ClientePortal';

  return (
    <AuthContext.Provider
      value={{
        user,
        token,
        login,
        logout,
        isAuthenticated,
        isAdmin,
        isColaborador,
        isCliente,
      }}
    >
      {children}
    </AuthContext.Provider>
  );
};

export const useAuth = () => {
  const context = useContext(AuthContext);
  if (!context) throw new Error('useAuth debe ser usado dentro de un AuthProvider');
  return context;
};

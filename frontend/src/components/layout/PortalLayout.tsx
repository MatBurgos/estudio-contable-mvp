import React from 'react';
import { Outlet } from 'react-router-dom';
import { useAuth } from '../../context/AuthContext';
import { Building2, LogOut, ShieldCheck } from 'lucide-react';

export const PortalLayout: React.FC = () => {
  const { user, logout } = useAuth();

  return (
    <div className="portal-container">
      <header className="portal-header">
        <div className="portal-brand">
          <Building2 size={24} className="text-primary" />
          <div>
            <h3>Portal del Cliente</h3>
            <small>Atendido por {user?.nombreEstudio}</small>
          </div>
        </div>

        <div className="portal-user">
          <div className="portal-badge">
            <ShieldCheck size={16} />
            <span>Acceso Seguro de Solo Lectura</span>
          </div>
          <span className="user-name">{user?.nombreCompleto}</span>
          <button onClick={logout} className="btn btn-outline-danger btn-sm">
            <LogOut size={14} /> Salir
          </button>
        </div>
      </header>

      <main className="portal-main">
        <Outlet />
      </main>

      <footer className="portal-footer">
        <p>© 2026 {user?.nombreEstudio} - Gestión Impositiva y Contable</p>
      </footer>
    </div>
  );
};

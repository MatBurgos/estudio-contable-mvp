import React from 'react';
import { NavLink, Outlet, useNavigate } from 'react-router-dom';
import { useAuth } from '../../context/AuthContext';
import { 
  LayoutDashboard, 
  Calendar, 
  Users, 
  CheckSquare, 
  FileText, 
  LogOut, 
  Building2, 
  UserCircle 
} from 'lucide-react';

export const AppLayout: React.FC = () => {
  const { user, logout } = useAuth();
  const navigate = useNavigate();

  return (
    <div className="app-container">
      {/* Sidebar */}
      <aside className="app-sidebar">
        <div className="sidebar-brand">
          <div className="brand-icon">
            <Building2 size={24} />
          </div>
          <div className="brand-text">
            <h2>{user?.nombreEstudio || 'Estudio Contable'}</h2>
            <span className="badge-tenant">SaaS Multi-Tenant</span>
          </div>
        </div>

        <nav className="sidebar-nav">
          <NavLink to="/dashboard" className={({ isActive }) => `nav-link ${isActive ? 'active' : ''}`}>
            <LayoutDashboard size={18} />
            <span>Dashboard</span>
          </NavLink>

          <NavLink to="/vencimientos" className={({ isActive }) => `nav-link ${isActive ? 'active' : ''}`}>
            <Calendar size={18} />
            <span>Vencimientos</span>
          </NavLink>

          <NavLink to="/clientes" className={({ isActive }) => `nav-link ${isActive ? 'active' : ''}`}>
            <Users size={18} />
            <span>Clientes</span>
          </NavLink>

          <NavLink to="/tareas" className={({ isActive }) => `nav-link ${isActive ? 'active' : ''}`}>
            <CheckSquare size={18} />
            <span>Tareas y Checklist</span>
          </NavLink>
        </nav>

        <div className="sidebar-footer">
          <div className="user-info">
            <UserCircle size={32} />
            <div className="user-details">
              <strong>{user?.nombreCompleto}</strong>
              <small>{user?.rol === 'AdminEstudio' ? 'Administrador' : 'Colaborador'}</small>
            </div>
          </div>
          <button onClick={logout} className="btn-logout" title="Cerrar sesión">
            <LogOut size={16} />
          </button>
        </div>
      </aside>

      {/* Main Content Area */}
      <main className="app-main">
        <header className="app-header">
          <div className="header-breadcrumbs">
            <span className="period-pill">
              Período Activo: <strong>{new Date().toLocaleDateString('es-AR', { month: 'long', year: 'numeric' })}</strong>
            </span>
          </div>
          <div className="header-actions">
            <button className="btn btn-secondary" onClick={() => navigate('/vencimientos')}>
              <Calendar size={15} /> Ver Calendario
            </button>
            <button className="btn btn-primary" onClick={() => navigate('/clientes')}>
              + Nuevo Cliente
            </button>
          </div>
        </header>

        <div className="content-scroll">
          <Outlet />
        </div>
      </main>
    </div>
  );
};

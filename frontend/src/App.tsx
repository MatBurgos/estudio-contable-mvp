import React from 'react';
import { BrowserRouter, Routes, Route, Navigate } from 'react-router-dom';
import { AuthProvider, useAuth } from './context/AuthContext';
import { AppLayout } from './components/layout/AppLayout';
import { PortalLayout } from './components/layout/PortalLayout';
import { LoginPage } from './pages/auth/LoginPage';
import { DashboardPage } from './pages/dashboard/DashboardPage';
import { VencimientosPage } from './pages/vencimientos/VencimientosPage';
import { ClientesPage } from './pages/clientes/ClientesPage';
import { ClienteDetailPage } from './pages/clientes/ClienteDetailPage';
import { TareasPage } from './pages/tareas/TareasPage';
import { PortalClientePage } from './pages/portal/PortalClientePage';

const ProtectedStudyRoute: React.FC<{ children: React.ReactNode }> = ({ children }) => {
  const { isAuthenticated, isCliente } = useAuth();

  if (!isAuthenticated) {
    return <Navigate to="/login" replace />;
  }

  if (isCliente) {
    return <Navigate to="/portal" replace />;
  }

  return <>{children}</>;
};

const ProtectedClientRoute: React.FC<{ children: React.ReactNode }> = ({ children }) => {
  const { isAuthenticated } = useAuth();

  if (!isAuthenticated) {
    return <Navigate to="/login" replace />;
  }

  return <>{children}</>;
};

export const App: React.FC = () => {
  return (
    <AuthProvider>
      <BrowserRouter>
        <Routes>
          <Route path="/login" element={<LoginPage />} />

          {/* Rutas del Estudio Contable */}
          <Route
            path="/"
            element={
              <ProtectedStudyRoute>
                <AppLayout />
              </ProtectedStudyRoute>
            }
          >
            <Route index element={<Navigate to="/dashboard" replace />} />
            <Route path="dashboard" element={<DashboardPage />} />
            <Route path="vencimientos" element={<VencimientosPage />} />
            <Route path="clientes" element={<ClientesPage />} />
            <Route path="clientes/:id" element={<ClienteDetailPage />} />
            <Route path="tareas" element={<TareasPage />} />
          </Route>

          {/* Rutas del Portal de Clientes */}
          <Route
            path="/portal"
            element={
              <ProtectedClientRoute>
                <PortalLayout />
              </ProtectedClientRoute>
            }
          >
            <Route index element={<PortalClientePage />} />
          </Route>

          <Route path="*" element={<Navigate to="/login" replace />} />
        </Routes>
      </BrowserRouter>
    </AuthProvider>
  );
};

export default App;

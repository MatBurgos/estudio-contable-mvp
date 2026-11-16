import React, { useEffect, useState } from 'react';
import { useNavigate } from 'react-router-dom';
import api from '../../api/client';
import { DashboardResumenDto } from '../../types';
import { 
  Users, 
  Calendar, 
  CheckCircle2, 
  Clock, 
  AlertTriangle, 
  ListChecks, 
  ArrowUpRight,
  TrendingUp,
  FileCheck2
} from 'lucide-react';

export const DashboardPage: React.FC = () => {
  const [data, setData] = useState<DashboardResumenDto | null>(null);
  const [loading, setLoading] = useState(true);
  const navigate = useNavigate();

  useEffect(() => {
    fetchDashboard();
  }, []);

  const fetchDashboard = async () => {
    setLoading(true);
    try {
      const res = await api.get<DashboardResumenDto>('/dashboard/resumen');
      setData(res.data);
    } catch (err) {
      console.error('Error cargando dashboard', err);
    } finally {
      setLoading(false);
    }
  };

  if (loading) {
    return <div className="loading-state">Cargando métricas del estudio...</div>;
  }

  const porcentajePresentado = data && data.vencimientosMesTotal > 0
    ? Math.round((data.vencimientosPresentados / data.vencimientosMesTotal) * 100)
    : 0;

  return (
    <div className="dashboard-page">
      <div className="page-header">
        <div>
          <h1>Panel de Control</h1>
          <p>Estado operativo de vencimientos impositivos y tareas del equipo</p>
        </div>
        <div className="page-actions">
          <button className="btn btn-secondary" onClick={fetchDashboard}>
            Actualizar
          </button>
          <button className="btn btn-primary" onClick={() => navigate('/vencimientos')}>
            Ver Matriz Completa
          </button>
        </div>
      </div>

      {/* KPI Cards */}
      <div className="kpi-grid">
        <div className="kpi-card">
          <div className="kpi-icon bg-blue-subtle">
            <Users size={22} className="text-blue" />
          </div>
          <div className="kpi-content">
            <span className="kpi-label">Clientes Activos</span>
            <span className="kpi-value">{data?.totalClientesActivos || 0}</span>
          </div>
        </div>

        <div className="kpi-card">
          <div className="kpi-icon bg-purple-subtle">
            <Calendar size={22} className="text-purple" />
          </div>
          <div className="kpi-content">
            <span className="kpi-label">Vencimientos del Mes</span>
            <span className="kpi-value">{data?.vencimientosMesTotal || 0}</span>
          </div>
        </div>

        <div className="kpi-card">
          <div className="kpi-icon bg-green-subtle">
            <CheckCircle2 size={22} className="text-green" />
          </div>
          <div className="kpi-content">
            <span className="kpi-label">Presentados / Pagados</span>
            <span className="kpi-value">{data?.vencimientosPresentados || 0}</span>
            <small className="kpi-hint text-green">
              <TrendingUp size={12} /> {porcentajePresentado}% completado
            </small>
          </div>
        </div>

        <div className="kpi-card">
          <div className="kpi-icon bg-yellow-subtle">
            <Clock size={22} className="text-yellow" />
          </div>
          <div className="kpi-content">
            <span className="kpi-label">En Proceso / Pendientes</span>
            <span className="kpi-value">{data?.vencimientosPendientes || 0}</span>
          </div>
        </div>

        <div className="kpi-card">
          <div className="kpi-icon bg-red-subtle">
            <AlertTriangle size={22} className="text-red" />
          </div>
          <div className="kpi-content">
            <span className="kpi-label">Vencidos sin Presentar</span>
            <span className="kpi-value text-red">{data?.vencimientosVencidos || 0}</span>
          </div>
        </div>

        <div className="kpi-card">
          <div className="kpi-icon bg-indigo-subtle">
            <ListChecks size={22} className="text-indigo" />
          </div>
          <div className="kpi-content">
            <span className="kpi-label">Tareas Pendientes</span>
            <span className="kpi-value">{data?.tareasPendientes || 0}</span>
          </div>
        </div>
      </div>

      {/* Main Grid: Vencimientos Urgentes & Tareas */}
      <div className="dashboard-content-grid">
        {/* Urgentes & Próximos */}
        <div className="card dashboard-card">
          <div className="card-header">
            <div>
              <h3>Próximos Vencimientos Inmediatos</h3>
              <p className="card-subtitle">Obligaciones a liquidar y presentar en los próximos días</p>
            </div>
            <button className="btn-link" onClick={() => navigate('/vencimientos')}>
              Ver todos <ArrowUpRight size={14} />
            </button>
          </div>

          <div className="table-responsive">
            <table className="table">
              <thead>
                <tr>
                  <th>Cliente</th>
                  <th>Impuesto</th>
                  <th>Vencimiento</th>
                  <th>Estado</th>
                  <th>Monto VEP</th>
                  <th>Acción</th>
                </tr>
              </thead>
              <tbody>
                {data?.proximosVencimientos && data.proximosVencimientos.length > 0 ? (
                  data.proximosVencimientos.map((v) => (
                    <tr key={v.id}>
                      <td>
                        <div className="cell-client">
                          <strong>{v.clienteRazonSocial}</strong>
                          <small className="text-muted font-mono">{v.clienteCuit}</small>
                        </div>
                      </td>
                      <td>
                        <span className="badge badge-tax">{v.obligacionNombre}</span>
                        {v.jurisdiccion && <small className="d-block text-muted">{v.jurisdiccion}</small>}
                      </td>
                      <td>
                        <div className="cell-date">
                          <strong>{v.fechaVencimientoEfectiva}</strong>
                          {v.tieneProrroga && <span className="badge badge-prorroga">Con Prórroga</span>}
                        </div>
                      </td>
                      <td>
                        <span className={`status-pill status-${v.estado.toLowerCase()}`}>
                          {v.estado}
                        </span>
                      </td>
                      <td>
                        {v.montoAPagar ? (
                          <strong className="text-currency">
                            ${v.montoAPagar.toLocaleString('es-AR', { minimumFractionDigits: 2 })}
                          </strong>
                        ) : (
                          <span className="text-muted">-</span>
                        )}
                      </td>
                      <td>
                        <button
                          className="btn btn-sm btn-outline"
                          onClick={() => navigate(`/clientes/${v.clienteId}`)}
                        >
                          Ficha
                        </button>
                      </td>
                    </tr>
                  ))
                ) : (
                  <tr>
                    <td colSpan={6} className="text-center py-4 text-muted">
                      No hay vencimientos pendientes en los próximos 15 días.
                    </td>
                  </tr>
                )}
              </tbody>
            </table>
          </div>
        </div>

        {/* Mis Tareas Pendientes */}
        <div className="card dashboard-card">
          <div className="card-header">
            <div>
              <h3>Checklist de Trabajo</h3>
              <p className="card-subtitle">Tareas prioritarias asignadas al equipo</p>
            </div>
            <button className="btn-link" onClick={() => navigate('/tareas')}>
              Ver checklist <ArrowUpRight size={14} />
            </button>
          </div>

          <div className="task-list">
            {data?.misTareasPendientes && data.misTareasPendientes.length > 0 ? (
              data.misTareasPendientes.map((t) => (
                <div key={t.id} className="task-item">
                  <div className="task-check">
                    <FileCheck2 size={18} className="text-primary" />
                  </div>
                  <div className="task-details">
                    <span className="task-title">{t.titulo}</span>
                    <div className="task-meta">
                      <span className="task-client">{t.clienteRazonSocial}</span>
                      {t.fechaLimite && (
                        <span className="task-due text-muted">Límite: {t.fechaLimite}</span>
                      )}
                      <span className={`badge-priority priority-${t.prioridad.toLowerCase()}`}>
                        {t.prioridad}
                      </span>
                    </div>
                  </div>
                </div>
              ))
            ) : (
              <div className="empty-tasks text-center py-4 text-muted">
                No tienes tareas pendientes asignadas actualmente.
              </div>
            )}
          </div>
        </div>
      </div>
    </div>
  );
};

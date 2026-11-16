import React, { useEffect, useState } from 'react';
import api from '../../api/client';
import { TareaDto, EstadoTarea, PrioridadTarea } from '../../types';
import { CheckSquare, CheckCircle2, Clock, Plus, Filter } from 'lucide-react';

export const TareasPage: React.FC = () => {
  const [tareas, setTareas] = useState<TareaDto[]>([]);
  const [loading, setLoading] = useState(true);
  const [filtroEstado, setFiltroEstado] = useState<string>('Pendiente');

  useEffect(() => {
    fetchTareas();
  }, [filtroEstado]);

  const fetchTareas = async () => {
    setLoading(true);
    try {
      const params: any = {};
      if (filtroEstado) params.estado = filtroEstado;

      const res = await api.get<TareaDto[]>('/tareas', { params });
      setTareas(res.data);
    } catch (err) {
      console.error('Error al cargar tareas', err);
    } finally {
      setLoading(false);
    }
  };

  const handleToggleCompletada = async (tarea: TareaDto) => {
    const nuevoEstado: EstadoTarea = tarea.estado === 'Completada' ? 'Pendiente' : 'Completada';
    try {
      await api.patch(`/tareas/${tarea.id}/estado`, { nuevoEstado });
      await fetchTareas();
    } catch (err) {
      alert('Error al actualizar tarea.');
    }
  };

  return (
    <div className="tareas-page">
      <div className="page-header">
        <div>
          <h1>Checklist y Tareas Operativas</h1>
          <p>Seguimiento de pasos por liquidación, pedidos de extractos y conciliaciones</p>
        </div>
      </div>

      {/* Filtros */}
      <div className="card filter-bar">
        <div className="filter-group">
          <label>Filtrar por Estado:</label>
          <select
            value={filtroEstado}
            onChange={(e) => setFiltroEstado(e.target.value)}
            className="input-select"
          >
            <option value="">Todas las Tareas</option>
            <option value="Pendiente">Pendientes</option>
            <option value="EnProceso">En Proceso</option>
            <option value="Completada">Completadas</option>
          </select>
        </div>
      </div>

      {/* Lista de Tareas */}
      <div className="card mt-4">
        {loading ? (
          <div className="py-8 text-center text-muted">Cargando checklist...</div>
        ) : (
          <div className="task-checklist-full">
            {tareas.length > 0 ? (
              tareas.map((t) => (
                <div key={t.id} className={`checklist-row ${t.estado === 'Completada' ? 'completed' : ''}`}>
                  <button
                    className={`btn-check ${t.estado === 'Completada' ? 'checked' : ''}`}
                    onClick={() => handleToggleCompletada(t)}
                    title="Marcar como completada / pendiente"
                  >
                    <CheckCircle2 size={20} />
                  </button>

                  <div className="checklist-info">
                    <span className="checklist-title">{t.titulo}</span>
                    <div className="checklist-sub">
                      <strong className="text-dark">{t.clienteRazonSocial}</strong>
                      {t.vencimientoInfo && <span className="text-muted">• {t.vencimientoInfo}</span>}
                      {t.asignadoANombre && <span className="text-muted">• Asignado a: {t.asignadoANombre}</span>}
                      {t.fechaLimite && <span className="text-primary font-mono text-xs">• Límite: {t.fechaLimite}</span>}
                    </div>
                  </div>

                  <div className="checklist-tags">
                    <span className={`badge-priority priority-${t.prioridad.toLowerCase()}`}>
                      {t.prioridad}
                    </span>
                    <span className={`status-pill status-${t.estado.toLowerCase()}`}>
                      {t.estado}
                    </span>
                  </div>
                </div>
              ))
            ) : (
              <div className="py-8 text-center text-muted">
                No hay tareas con este criterio de filtro.
              </div>
            )}
          </div>
        )}
      </div>
    </div>
  );
};

import React, { useEffect, useState } from 'react';
import { useNavigate } from 'react-router-dom';
import api from '../../api/client';
import { VencimientoDto, EstadoVencimiento } from '../../types';
import { 
  Calendar, 
  Search, 
  Filter, 
  PlusCircle, 
  Clock, 
  CheckCircle, 
  AlertCircle,
  FileText,
  DollarSign
} from 'lucide-react';

export const VencimientosPage: React.FC = () => {
  const [vencimientos, setVencimientos] = useState<VencimientoDto[]>([]);
  const [loading, setLoading] = useState(true);
  const [periodo, setPeriodo] = useState<string>(() => {
    const now = new Date();
    return `${now.getFullYear()}-${String(now.getMonth() + 1).padStart(2, '0')}`;
  });
  const [estadoFiltro, setEstadoFiltro] = useState<string>('');
  const [busqueda, setBusqueda] = useState<string>('');
  const [generando, setGenerando] = useState(false);

  // Modal de Prórroga
  const [modalProrroga, setModalProrroga] = useState<{ open: boolean; item?: VencimientoDto; nuevaFecha: string; motivo: string }>({
    open: false,
    nuevaFecha: '',
    motivo: '',
  });

  // Modal de Estado / Monto VEP
  const [modalEstado, setModalEstado] = useState<{ open: boolean; item?: VencimientoDto; nuevoEstado: EstadoVencimiento; monto: string }>({
    open: false,
    nuevoEstado: 'Presentado',
    monto: '',
  });

  const navigate = useNavigate();

  useEffect(() => {
    fetchVencimientos();
  }, [periodo, estadoFiltro]);

  const fetchVencimientos = async () => {
    setLoading(true);
    try {
      const params: any = { periodo };
      if (estadoFiltro) params.estado = estadoFiltro;

      const res = await api.get<VencimientoDto[]>('/vencimientos', { params });
      setVencimientos(res.data);
    } catch (err) {
      console.error('Error al cargar vencimientos', err);
    } finally {
      setLoading(false);
    }
  };

  const handleGenerarPeriodo = async () => {
    setGenerando(true);
    try {
      const res = await api.post('/vencimientos/generar-periodo', { periodoFiscal: periodo });
      alert(res.data.message || 'Vencimientos generados exitosamente.');
      await fetchVencimientos();
    } catch (err: any) {
      alert(err.response?.data?.message || 'Error al generar vencimientos.');
    } finally {
      setGenerando(false);
    }
  };

  const handleGuardarProrroga = async (e: React.FormEvent) => {
    e.preventDefault();
    if (!modalProrroga.item) return;

    try {
      await api.patch(`/vencimientos/${modalProrroga.item.id}/prorroga`, {
        nuevaFechaEfectiva: modalProrroga.nuevaFecha,
        motivo: modalProrroga.motivo,
      });
      setModalProrroga({ open: false, nuevaFecha: '', motivo: '' });
      await fetchVencimientos();
    } catch (err: any) {
      alert(err.response?.data?.message || 'Error al guardar prórroga.');
    }
  };

  const handleGuardarEstado = async (e: React.FormEvent) => {
    e.preventDefault();
    if (!modalEstado.item) return;

    try {
      const montoNum = modalEstado.monto ? parseFloat(modalEstado.monto) : null;
      await api.patch(`/vencimientos/${modalEstado.item.id}/estado`, {
        nuevoEstado: modalEstado.nuevoEstado,
        montoAPagar: montoNum,
      });
      setModalEstado({ open: false, nuevoEstado: 'Presentado', monto: '' });
      await fetchVencimientos();
    } catch (err: any) {
      alert(err.response?.data?.message || 'Error al actualizar estado.');
    }
  };

  const vencimientosFiltrados = vencimientos.filter((v) => {
    if (!busqueda) return true;
    const b = busqueda.toLowerCase();
    return (
      v.clienteRazonSocial.toLowerCase().includes(b) ||
      v.clienteCuit.includes(b) ||
      v.obligacionNombre.toLowerCase().includes(b)
    );
  });

  return (
    <div className="vencimientos-page">
      <div className="page-header">
        <div>
          <h1>Matriz de Vencimientos Impositivos</h1>
          <p>Control cronológico por CUIT, prórrogas y seguimiento de liquidaciones</p>
        </div>
        <div className="page-actions">
          <button
            className="btn btn-primary"
            onClick={handleGenerarPeriodo}
            disabled={generando}
          >
            <PlusCircle size={16} />
            {generando ? 'Generando...' : `Generar Vencimientos ${periodo}`}
          </button>
        </div>
      </div>

      {/* Barra de Filtros */}
      <div className="card filter-bar">
        <div className="filter-group">
          <label>Período Fiscal:</label>
          <input
            type="month"
            value={periodo}
            onChange={(e) => setPeriodo(e.target.value)}
            className="input-select"
          />
        </div>

        <div className="filter-group">
          <label>Estado:</label>
          <select
            value={estadoFiltro}
            onChange={(e) => setEstadoFiltro(e.target.value)}
            className="input-select"
          >
            <option value="">Todos los Estados</option>
            <option value="Pendiente">Pendiente</option>
            <option value="EnProceso">En Proceso</option>
            <option value="Presentado">Presentado</option>
            <option value="Pagado">Pagado</option>
            <option value="Vencido">Vencido</option>
          </select>
        </div>

        <div className="filter-search">
          <Search size={16} />
          <input
            type="text"
            placeholder="Buscar por cliente, CUIT o impuesto..."
            value={busqueda}
            onChange={(e) => setBusqueda(e.target.value)}
          />
        </div>
      </div>

      {/* Tabla de Vencimientos */}
      <div className="card mt-4">
        {loading ? (
          <div className="py-8 text-center text-muted">Cargando matriz de vencimientos...</div>
        ) : (
          <div className="table-responsive">
            <table className="table table-hover">
              <thead>
                <tr>
                  <th>Cliente</th>
                  <th>Impuesto / Tributo</th>
                  <th>Fecha Original</th>
                  <th>Fecha Efectiva</th>
                  <th>Tareas Checklist</th>
                  <th>Estado</th>
                  <th>Monto VEP</th>
                  <th>Acciones</th>
                </tr>
              </thead>
              <tbody>
                {vencimientosFiltrados.length > 0 ? (
                  vencimientosFiltrados.map((v) => (
                    <tr key={v.id}>
                      <td>
                        <div className="cell-client">
                          <strong className="cursor-pointer" onClick={() => navigate(`/clientes/${v.clienteId}`)}>
                            {v.clienteRazonSocial}
                          </strong>
                          <span className="font-mono text-muted text-xs">CUIT: {v.clienteCuit}</span>
                        </div>
                      </td>
                      <td>
                        <span className="badge badge-tax">{v.obligacionNombre}</span>
                        {v.jurisdiccion && (
                          <div className="text-xs text-muted mt-1">{v.jurisdiccion}</div>
                        )}
                      </td>
                      <td>
                        <span className="text-muted">{v.fechaVencimientoOriginal}</span>
                      </td>
                      <td>
                        <div className="cell-date">
                          <strong>{v.fechaVencimientoEfectiva}</strong>
                          {v.tieneProrroga && (
                            <span className="badge badge-prorroga" title={v.motivoProrroga}>
                              Prórroga
                            </span>
                          )}
                        </div>
                      </td>
                      <td>
                        <div className="checklist-progress">
                          <span className="text-xs font-semibold">
                            {v.tareasCompletadas}/{v.totalTareas}
                          </span>
                          <div className="progress-bar-bg">
                            <div
                              className="progress-bar-fill"
                              style={{
                                width: `${v.totalTareas > 0 ? (v.tareasCompletadas / v.totalTareas) * 100 : 0}%`,
                              }}
                            />
                          </div>
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
                          <span className="text-muted text-xs">Sin liquidar</span>
                        )}
                      </td>
                      <td>
                        <div className="action-buttons">
                          <button
                            className="btn btn-sm btn-outline"
                            title="Cambiar estado o cargar VEP"
                            onClick={() =>
                              setModalEstado({
                                open: true,
                                item: v,
                                nuevoEstado: v.estado,
                                monto: v.montoAPagar ? String(v.montoAPagar) : '',
                              })
                            }
                          >
                            Estado
                          </button>
                          <button
                            className="btn btn-sm btn-outline-warning"
                            title="Modificar fecha por prórroga"
                            onClick={() =>
                              setModalProrroga({
                                open: true,
                                item: v,
                                nuevaFecha: v.fechaVencimientoEfectiva,
                                motivo: v.motivoProrroga || '',
                              })
                            }
                          >
                            Prórroga
                          </button>
                        </div>
                      </td>
                    </tr>
                  ))
                ) : (
                  <tr>
                    <td colSpan={8} className="py-8 text-center text-muted">
                      No se encontraron vencimientos para este período. Pulsa <strong>"Generar Vencimientos"</strong> para instanciar el mes según los CUITs de tus clientes.
                    </td>
                  </tr>
                )}
              </tbody>
            </table>
          </div>
        )}
      </div>

      {/* Modal de Prórroga */}
      {modalProrroga.open && modalProrroga.item && (
        <div className="modal-backdrop">
          <div className="modal-card">
            <h3>Registrar Prórroga de Vencimiento</h3>
            <p className="text-muted">
              {modalProrroga.item.clienteRazonSocial} - {modalProrroga.item.obligacionNombre}
            </p>

            <form onSubmit={handleGuardarProrroga}>
              <div className="form-group mt-3">
                <label>Nueva Fecha de Vencimiento Efectiva:</label>
                <input
                  type="date"
                  required
                  value={modalProrroga.nuevaFecha}
                  onChange={(e) => setModalProrroga({ ...modalProrroga, nuevaFecha: e.target.value })}
                />
              </div>

              <div className="form-group">
                <label>Motivo de la Prórroga:</label>
                <textarea
                  required
                  rows={3}
                  placeholder="ej. Prórroga general dispuesta por Resolución General ARCA N° ..."
                  value={modalProrroga.motivo}
                  onChange={(e) => setModalProrroga({ ...modalProrroga, motivo: e.target.value })}
                />
              </div>

              <div className="modal-actions">
                <button
                  type="button"
                  className="btn btn-secondary"
                  onClick={() => setModalProrroga({ open: false, nuevaFecha: '', motivo: '' })}
                >
                  Cancelar
                </button>
                <button type="submit" className="btn btn-primary">
                  Confirmar Prórroga
                </button>
              </div>
            </form>
          </div>
        </div>
      )}

      {/* Modal de Estado / Monto VEP */}
      {modalEstado.open && modalEstado.item && (
        <div className="modal-backdrop">
          <div className="modal-card">
            <h3>Actualizar Estado de Liquidación</h3>
            <p className="text-muted">
              {modalEstado.item.clienteRazonSocial} - {modalEstado.item.obligacionNombre}
            </p>

            <form onSubmit={handleGuardarEstado}>
              <div className="form-group mt-3">
                <label>Nuevo Estado:</label>
                <select
                  value={modalEstado.nuevoEstado}
                  onChange={(e) =>
                    setModalEstado({
                      ...modalEstado,
                      nuevoEstado: e.target.value as EstadoVencimiento,
                    })
                  }
                >
                  <option value="Pendiente">Pendiente</option>
                  <option value="EnProceso">En Proceso</option>
                  <option value="Presentado">Presentado (DJ presentada)</option>
                  <option value="Pagado">Pagado (VEP cancelado)</option>
                  <option value="Vencido">Vencido</option>
                </select>
              </div>

              <div className="form-group">
                <label>Monto a Pagar / Saldo VEP ($ ARS):</label>
                <input
                  type="number"
                  step="0.01"
                  placeholder="0.00"
                  value={modalEstado.monto}
                  onChange={(e) => setModalEstado({ ...modalEstado, monto: e.target.value })}
                />
              </div>

              <div className="modal-actions">
                <button
                  type="button"
                  className="btn btn-secondary"
                  onClick={() => setModalEstado({ open: false, nuevoEstado: 'Presentado', monto: '' })}
                >
                  Cancelar
                </button>
                <button type="submit" className="btn btn-primary">
                  Guardar Cambios
                </button>
              </div>
            </form>
          </div>
        </div>
      )}
    </div>
  );
};

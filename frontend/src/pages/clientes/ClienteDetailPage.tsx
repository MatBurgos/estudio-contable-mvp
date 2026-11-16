import React, { useEffect, useState } from 'react';
import { useParams, useNavigate } from 'react-router-dom';
import api from '../../api/client';
import { ClienteDetailDto, VencimientoDto, DocumentoDto, TareaDto } from '../../types';
import { 
  Building, 
  ArrowLeft, 
  Calendar, 
  FileText, 
  CheckSquare, 
  Upload, 
  Plus, 
  Mail, 
  Phone,
  Download,
  Trash2
} from 'lucide-react';

export const ClienteDetailPage: React.FC = () => {
  const { id } = useParams<{ id: string }>();
  const navigate = useNavigate();

  const [cliente, setCliente] = useState<ClienteDetailDto | null>(null);
  const [vencimientos, setVencimientos] = useState<VencimientoDto[]>([]);
  const [documentos, setDocumentos] = useState<DocumentoDto[]>([]);
  const [tareas, setTareas] = useState<TareaDto[]>([]);
  const [activeTab, setActiveTab] = useState<'obligaciones' | 'vencimientos' | 'documentos' | 'tareas'>('vencimientos');
  const [loading, setLoading] = useState(true);

  // Subida de documento
  const [uploadFile, setUploadFile] = useState<File | null>(null);
  const [categoriaDoc, setCategoriaDoc] = useState('AcusePresentacion');
  const [subiendo, setSubiendo] = useState(false);

  useEffect(() => {
    if (id) {
      cargarDatos();
    }
  }, [id]);

  const cargarDatos = async () => {
    setLoading(true);
    try {
      const [resCliente, resVenc, resDocs, resTareas] = await Promise.all([
        api.get<ClienteDetailDto>(`/clientes/${id}`),
        api.get<VencimientoDto[]>('/vencimientos', { params: { clienteId: id } }),
        api.get<DocumentoDto[]>('/documentos', { params: { clienteId: id } }),
        api.get<TareaDto[]>('/tareas', { params: { clienteId: id } }),
      ]);

      setCliente(resCliente.data);
      setVencimientos(resVenc.data);
      setDocumentos(resDocs.data);
      setTareas(resTareas.data);
    } catch (err) {
      console.error('Error cargando ficha del cliente', err);
    } finally {
      setLoading(false);
    }
  };

  const handleSubirDocumento = async (e: React.FormEvent) => {
    e.preventDefault();
    if (!uploadFile || !id) return;

    setSubiendo(true);
    try {
      const formData = new FormData();
      formData.append('file', uploadFile);
      formData.append('clienteId', id);
      formData.append('categoria', categoriaDoc);
      formData.append('visibleParaCliente', 'true');

      await api.post('/documentos/upload', formData, {
        headers: { 'Content-Type': 'multipart/form-data' },
      });

      setUploadFile(null);
      alert('Documento subido correctamente.');
      const resDocs = await api.get<DocumentoDto[]>('/documentos', { params: { clienteId: id } });
      setDocumentos(resDocs.data);
    } catch (err: any) {
      alert(err.response?.data?.message || 'Error al subir documento.');
    } finally {
      setSubiendo(false);
    }
  };

  const handleDescargarDoc = (docId: string) => {
    window.open(`/api/documentos/${docId}/download`, '_blank');
  };

  if (loading) {
    return <div className="py-12 text-center text-muted">Cargando ficha del cliente...</div>;
  }

  if (!cliente) {
    return <div className="py-12 text-center text-red">Cliente no encontrado.</div>;
  }

  return (
    <div className="cliente-detail-page">
      <div className="page-header">
        <div className="d-flex items-center gap-3">
          <button className="btn btn-outline btn-sm" onClick={() => navigate('/clientes')}>
            <ArrowLeft size={16} /> Volver
          </button>
          <div>
            <h1>{cliente.razonSocial}</h1>
            <p className="font-mono text-muted">CUIT: {cliente.cuit} • {cliente.regimenTributario}</p>
          </div>
        </div>
      </div>

      {/* Tabs */}
      <div className="detail-tabs mt-4">
        <button
          className={`tab-btn ${activeTab === 'vencimientos' ? 'active' : ''}`}
          onClick={() => setActiveTab('vencimientos')}
        >
          <Calendar size={16} /> Vencimientos ({vencimientos.length})
        </button>
        <button
          className={`tab-btn ${activeTab === 'obligaciones' ? 'active' : ''}`}
          onClick={() => setActiveTab('obligaciones')}
        >
          <Building size={16} /> Impuestos Activos ({cliente.obligaciones.length})
        </button>
        <button
          className={`tab-btn ${activeTab === 'documentos' ? 'active' : ''}`}
          onClick={() => setActiveTab('documentos')}
        >
          <FileText size={16} /> Documentos y VEPs ({documentos.length})
        </button>
        <button
          className={`tab-btn ${activeTab === 'tareas' ? 'active' : ''}`}
          onClick={() => setActiveTab('tareas')}
        >
          <CheckSquare size={16} /> Checklist ({tareas.length})
        </button>
      </div>

      <div className="tab-content mt-4">
        {/* Tab Vencimientos */}
        {activeTab === 'vencimientos' && (
          <div className="card">
            <h3>Historial de Vencimientos</h3>
            <div className="table-responsive mt-3">
              <table className="table">
                <thead>
                  <tr>
                    <th>Período</th>
                    <th>Impuesto</th>
                    <th>Fecha Límite</th>
                    <th>Estado</th>
                    <th>Monto VEP</th>
                    <th>Tareas</th>
                  </tr>
                </thead>
                <tbody>
                  {vencimientos.length > 0 ? (
                    vencimientos.map((v) => (
                      <tr key={v.id}>
                        <td><strong>{v.periodoFiscal}</strong></td>
                        <td>{v.obligacionNombre}</td>
                        <td>
                          {v.fechaVencimientoEfectiva}
                          {v.tieneProrroga && <span className="badge badge-prorroga ms-2">Prórroga</span>}
                        </td>
                        <td><span className={`status-pill status-${v.estado.toLowerCase()}`}>{v.estado}</span></td>
                        <td>{v.montoAPagar ? `$${v.montoAPagar.toLocaleString('es-AR')}` : '-'}</td>
                        <td>{v.tareasCompletadas}/{v.totalTareas} completadas</td>
                      </tr>
                    ))
                  ) : (
                    <tr>
                      <td colSpan={6} className="text-center py-4 text-muted">
                        No hay vencimientos registrados para este cliente.
                      </td>
                    </tr>
                  )}
                </tbody>
              </table>
            </div>
          </div>
        )}

        {/* Tab Impuestos Activos */}
        {activeTab === 'obligaciones' && (
          <div className="card">
            <h3>Impuestos y Regímenes Inscriptos</h3>
            <div className="obligaciones-list mt-3">
              {cliente.obligaciones.map((o) => (
                <div key={o.id} className="obligacion-card">
                  <div>
                    <strong>{o.nombre}</strong>
                    <div className="text-muted text-xs">Organismo: {o.organismo} • Periodicidad: {o.periodicidad}</div>
                    {o.jurisdiccion && <div className="text-xs text-primary mt-1">Jurisdicción: {o.jurisdiccion}</div>}
                  </div>
                  <span className="badge badge-success">Activo</span>
                </div>
              ))}
            </div>
          </div>
        )}

        {/* Tab Documentos */}
        {activeTab === 'documentos' && (
          <div className="card">
            <h3>Documentos del Cliente (Acuses, VEPs, Extractos)</h3>

            {/* Formulario de subida rápida */}
            <form onSubmit={handleSubirDocumento} className="upload-box mt-3 mb-4">
              <div className="d-flex items-center gap-3 flex-wrap">
                <input
                  type="file"
                  required
                  onChange={(e) => setUploadFile(e.target.files ? e.target.files[0] : null)}
                />
                <select value={categoriaDoc} onChange={(e) => setCategoriaDoc(e.target.value)}>
                  <option value="AcusePresentacion">Acuse de Presentación DJ</option>
                  <option value="ComprobantePago_VEP">Volante de Pago (VEP)</option>
                  <option value="ExtractoBancario">Extracto Bancario</option>
                  <option value="FacturaVentas">Facturas de Venta</option>
                  <option value="FacturaCompras">Facturas de Compra</option>
                  <option value="Otro">Otro Comprobante</option>
                </select>
                <button type="submit" className="btn btn-primary btn-sm" disabled={subiendo || !uploadFile}>
                  <Upload size={14} /> {subiendo ? 'Subiendo...' : 'Subir Archivo'}
                </button>
              </div>
            </form>

            <div className="table-responsive">
              <table className="table">
                <thead>
                  <tr>
                    <th>Nombre Archivo</th>
                    <th>Categoría</th>
                    <th>Subido Por</th>
                    <th>Fecha</th>
                    <th>Acción</th>
                  </tr>
                </thead>
                <tbody>
                  {documentos.length > 0 ? (
                    documentos.map((d) => (
                      <tr key={d.id}>
                        <td><strong>{d.nombreOriginal}</strong></td>
                        <td><span className="badge badge-outline">{d.categoria}</span></td>
                        <td>{d.subidoPorNombre}</td>
                        <td>{new Date(d.createdAt).toLocaleDateString('es-AR')}</td>
                        <td>
                          <button
                            className="btn btn-sm btn-outline"
                            onClick={() => handleDescargarDoc(d.id)}
                          >
                            <Download size={13} /> Descargar
                          </button>
                        </td>
                      </tr>
                    ))
                  ) : (
                    <tr>
                      <td colSpan={5} className="text-center py-4 text-muted">
                        No hay documentos subidos para este cliente.
                      </td>
                    </tr>
                  )}
                </tbody>
              </table>
            </div>
          </div>
        )}

        {/* Tab Tareas */}
        {activeTab === 'tareas' && (
          <div className="card">
            <h3>Checklist Operativo</h3>
            <div className="task-list mt-3">
              {tareas.length > 0 ? (
                tareas.map((t) => (
                  <div key={t.id} className="task-item">
                    <div className="task-details">
                      <strong>{t.titulo}</strong>
                      {t.vencimientoInfo && <span className="text-muted text-xs ms-2">({t.vencimientoInfo})</span>}
                      <div className="task-meta mt-1">
                        <span className={`status-pill status-${t.estado.toLowerCase()}`}>{t.estado}</span>
                        <span className={`badge-priority priority-${t.prioridad.toLowerCase()}`}>{t.prioridad}</span>
                      </div>
                    </div>
                  </div>
                ))
              ) : (
                <div className="py-4 text-center text-muted">No hay tareas pendientes para este cliente.</div>
              )}
            </div>
          </div>
        )}
      </div>
    </div>
  );
};

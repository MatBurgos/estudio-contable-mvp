import React, { useEffect, useState } from 'react';
import api from '../../api/client';
import { PortalResumenDto, PortalVencimientoDto, PortalDocumentoDto } from '../../types';
import { 
  Building2, 
  CheckCircle2, 
  Clock, 
  Download, 
  DollarSign, 
  FileCheck, 
  AlertCircle,
  FileText
} from 'lucide-react';

export const PortalClientePage: React.FC = () => {
  const [data, setData] = useState<PortalResumenDto | null>(null);
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    fetchPortalData();
  }, []);

  const fetchPortalData = async () => {
    setLoading(true);
    try {
      const res = await api.get<PortalResumenDto>('/portal-cliente/resumen');
      setData(res.data);
    } catch (err) {
      console.error('Error cargando portal del cliente', err);
    } finally {
      setLoading(false);
    }
  };

  const handleDescargar = (docId: string) => {
    window.open(`/api/portal-cliente/documentos/${docId}/download`, '_blank');
  };

  if (loading) {
    return <div className="py-12 text-center text-muted">Cargando estado de sus trámites...</div>;
  }

  if (!data) {
    return (
      <div className="alert alert-error">
        <AlertCircle size={20} />
        <span>No se pudo obtener la información de su cuenta. Por favor contáctese con su estudio contable.</span>
      </div>
    );
  }

  return (
    <div className="portal-page">
      {/* Bienvenida y Datos Fiscales */}
      <div className="portal-banner">
        <div className="portal-banner-info">
          <h1>{data.razonSocial}</h1>
          <p className="font-mono text-muted">
            CUIT: {data.cuit} • Régimen: {data.regimenTributario}
          </p>
        </div>
        <div className="portal-banner-badge">
          <span>Período {data.periodoActual}</span>
        </div>
      </div>

      {/* Resumen de Estado */}
      <div className="portal-kpi-grid mt-4">
        <div className="portal-kpi-card">
          <div className="portal-kpi-title">
            <Clock size={18} className="text-yellow" />
            <span>En Proceso</span>
          </div>
          <span className="portal-kpi-number">{data.tramitesEnProceso}</span>
          <small className="text-muted">Liquidaciones en preparación</small>
        </div>

        <div className="portal-kpi-card">
          <div className="portal-kpi-title">
            <CheckCircle2 size={18} className="text-green" />
            <span>Presentados</span>
          </div>
          <span className="portal-kpi-number text-green">{data.tramitesPresentados}</span>
          <small className="text-muted">Declaraciones juradas listas</small>
        </div>

        <div className="portal-kpi-card">
          <div className="portal-kpi-title">
            <DollarSign size={18} className="text-primary" />
            <span>Monto Total a Pagar</span>
          </div>
          <span className="portal-kpi-number text-currency">
            ${data.totalMontoPendientePago.toLocaleString('es-AR', { minimumFractionDigits: 2 })}
          </span>
          <small className="text-muted">Suma de VEPs emitidos para pago</small>
        </div>
      </div>

      {/* Lista de Trámites y Vencimientos */}
      <div className="card mt-4">
        <div className="card-header">
          <div>
            <h3>Estado de sus Impuestos (Mes en curso)</h3>
            <p className="card-subtitle">Consulte la fecha límite de vencimiento y descargue sus comprobantes</p>
          </div>
        </div>

        <div className="table-responsive">
          <table className="table">
            <thead>
              <tr>
                <th>Impuesto / Obligación</th>
                <th>Fecha Vencimiento</th>
                <th>Estado del Trámite</th>
                <th>Monto a Pagar (VEP)</th>
                <th>Comprobantes Disponibles</th>
              </tr>
            </thead>
            <tbody>
              {data.vencimientosDelMes && data.vencimientosDelMes.length > 0 ? (
                data.vencimientosDelMes.map((v) => (
                  <tr key={v.id}>
                    <td>
                      <div className="d-flex items-center gap-2">
                        <FileCheck size={18} className="text-primary" />
                        <strong>{v.impuesto}</strong>
                      </div>
                    </td>
                    <td>
                      <span className="font-semibold">{v.fechaVencimiento}</span>
                    </td>
                    <td>
                      <span className={`status-pill status-${v.estado.toLowerCase()}`}>
                        {v.estado === 'Presentado' ? 'Presentado (Listo para pagar)' : v.estado}
                      </span>
                    </td>
                    <td>
                      {v.montoAPagar ? (
                        <strong className="text-currency">
                          ${v.montoAPagar.toLocaleString('es-AR', { minimumFractionDigits: 2 })}
                        </strong>
                      ) : (
                        <span className="text-muted text-xs">Liquidación pendiente</span>
                      )}
                    </td>
                    <td>
                      <div className="d-flex gap-2 flex-wrap">
                        {v.documentosDisponibles && v.documentosDisponibles.length > 0 ? (
                          v.documentosDisponibles.map((doc) => (
                            <button
                              key={doc.id}
                              className="btn btn-sm btn-outline-primary"
                              onClick={() => handleDescargar(doc.id)}
                            >
                              <Download size={12} /> {doc.categoria === 'ComprobantePago_VEP' ? 'Descargar VEP' : 'Acuse DJ'}
                            </button>
                          ))
                        ) : (
                          <span className="text-muted text-xs">Sin documentos aún</span>
                        )}
                      </div>
                    </td>
                  </tr>
                ))
              ) : (
                <tr>
                  <td colSpan={5} className="text-center py-6 text-muted">
                    No posee obligaciones impositivas activas para este período.
                  </td>
                </tr>
              )}
            </tbody>
          </table>
        </div>
      </div>
    </div>
  );
};

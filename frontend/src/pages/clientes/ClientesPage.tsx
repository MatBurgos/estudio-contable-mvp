import React, { useEffect, useState } from 'react';
import { useNavigate } from 'react-router-dom';
import api from '../../api/client';
import { ClienteListDto, TipoPersona, RegimenTributario } from '../../types';
import { Users, Search, Plus, Building, Phone, Mail, ArrowRight } from 'lucide-react';

export const ClientesPage: React.FC = () => {
  const [clientes, setClientes] = useState<ClienteListDto[]>([]);
  const [loading, setLoading] = useState(true);
  const [busqueda, setBusqueda] = useState('');
  const [modalNuevo, setModalNuevo] = useState(false);

  // Formulario nuevo cliente
  const [nuevoCliente, setNuevoCliente] = useState({
    cuit: '',
    razonSocial: '',
    nombreFantasia: '',
    tipoPersona: 'Juridica' as TipoPersona,
    regimenTributario: 'ResponsableInscripto' as RegimenTributario,
    emailContacto: '',
    telefonoContacto: '',
    notas: '',
    obligaciones: ['IVA', 'F931_LSD'] as string[],
  });

  const navigate = useNavigate();

  useEffect(() => {
    fetchClientes();
  }, []);

  const fetchClientes = async () => {
    setLoading(true);
    try {
      const res = await api.get<ClienteListDto[]>('/clientes');
      setClientes(res.data);
    } catch (err) {
      console.error('Error cargando clientes', err);
    } finally {
      setLoading(false);
    }
  };

  const handleCrearCliente = async (e: React.FormEvent) => {
    e.preventDefault();
    try {
      const cuitLimpio = nuevoCliente.cuit.replace(/[^0-9]/g, '');
      if (cuitLimpio.length !== 11) {
        alert('El CUIT debe tener exactamente 11 dígitos.');
        return;
      }

      await api.post('/clientes', {
        ...nuevoCliente,
        cuit: cuitLimpio,
        obligacionesIdsIniciales: nuevoCliente.obligaciones,
      });

      setModalNuevo(false);
      setNuevoCliente({
        cuit: '',
        razonSocial: '',
        nombreFantasia: '',
        tipoPersona: 'Juridica',
        regimenTributario: 'ResponsableInscripto',
        emailContacto: '',
        telefonoContacto: '',
        notas: '',
        obligaciones: ['IVA', 'F931_LSD'],
      });
      await fetchClientes();
    } catch (err: any) {
      alert(err.response?.data?.message || 'Error al dar de alta el cliente.');
    }
  };

  const toggleObligacion = (id: string) => {
    setNuevoCliente((prev) => {
      const exists = prev.obligaciones.includes(id);
      return {
        ...prev,
        obligaciones: exists
          ? prev.obligaciones.filter((o) => o !== id)
          : [...prev.obligaciones, id],
      };
    });
  };

  const clientesFiltrados = clientes.filter((c) => {
    if (!busqueda) return true;
    const b = busqueda.toLowerCase();
    return (
      c.razonSocial.toLowerCase().includes(b) ||
      c.cuit.includes(b) ||
      (c.nombreFantasia && c.nombreFantasia.toLowerCase().includes(b))
    );
  });

  return (
    <div className="clientes-page">
      <div className="page-header">
        <div>
          <h1>Gestión de Clientes</h1>
          <p>Fichero fiscal, asignación de impuestos y responsables del estudio</p>
        </div>
        <div className="page-actions">
          <button className="btn btn-primary" onClick={() => setModalNuevo(true)}>
            <Plus size={16} /> Nuevo Cliente
          </button>
        </div>
      </div>

      {/* Buscador */}
      <div className="card filter-bar">
        <div className="filter-search w-full">
          <Search size={16} />
          <input
            type="text"
            placeholder="Buscar por Razón Social, CUIT o Nombre de Fantasía..."
            value={busqueda}
            onChange={(e) => setBusqueda(e.target.value)}
          />
        </div>
      </div>

      {/* Lista / Grid de Clientes */}
      <div className="card mt-4">
        {loading ? (
          <div className="py-8 text-center text-muted">Cargando nómina de clientes...</div>
        ) : (
          <div className="table-responsive">
            <table className="table table-hover">
              <thead>
                <tr>
                  <th>CUIT / Razón Social</th>
                  <th>Tipo y Régimen</th>
                  <th>Contacto</th>
                  <th>Impuestos Activos</th>
                  <th>Responsable</th>
                  <th>Acción</th>
                </tr>
              </thead>
              <tbody>
                {clientesFiltrados.length > 0 ? (
                  clientesFiltrados.map((c) => (
                    <tr key={c.id}>
                      <td>
                        <div className="cell-client">
                          <strong className="cursor-pointer" onClick={() => navigate(`/clientes/${c.id}`)}>
                            {c.razonSocial}
                          </strong>
                          {c.nombreFantasia && (
                            <small className="text-muted">({c.nombreFantasia})</small>
                          )}
                          <span className="font-mono text-muted text-xs">CUIT: {c.cuit}</span>
                        </div>
                      </td>
                      <td>
                        <div className="d-flex flex-col gap-1">
                          <span className="badge badge-outline">{c.tipoPersona}</span>
                          <span className="badge badge-regimen">{c.regimenTributario}</span>
                        </div>
                      </td>
                      <td>
                        <div className="text-xs text-muted">
                          {c.emailContacto && (
                            <div className="d-flex items-center gap-1">
                              <Mail size={12} /> {c.emailContacto}
                            </div>
                          )}
                          {c.telefonoContacto && (
                            <div className="d-flex items-center gap-1 mt-1">
                              <Phone size={12} /> {c.telefonoContacto}
                            </div>
                          )}
                        </div>
                      </td>
                      <td>
                        <span className="badge badge-tax">
                          {c.cantidadObligaciones} {c.cantidadObligaciones === 1 ? 'impuesto' : 'impuestos'}
                        </span>
                      </td>
                      <td>
                        <span className="text-sm">{c.responsableNombre || 'Sin asignar'}</span>
                      </td>
                      <td>
                        <button
                          className="btn btn-sm btn-outline"
                          onClick={() => navigate(`/clientes/${c.id}`)}
                        >
                          Ver Ficha <ArrowRight size={13} />
                        </button>
                      </td>
                    </tr>
                  ))
                ) : (
                  <tr>
                    <td colSpan={6} className="py-8 text-center text-muted">
                      No se encontraron clientes que coincidan con la búsqueda.
                    </td>
                  </tr>
                )}
              </tbody>
            </table>
          </div>
        )}
      </div>

      {/* Modal Alta de Cliente */}
      {modalNuevo && (
        <div className="modal-backdrop">
          <div className="modal-card modal-lg">
            <h3>Alta de Nuevo Cliente</h3>
            <p className="text-muted">Complete los datos fiscales y seleccione los impuestos aplicables.</p>

            <form onSubmit={handleCrearCliente}>
              <div className="form-grid-2 mt-3">
                <div className="form-group">
                  <label>CUIT (11 dígitos, sin guiones):</label>
                  <input
                    type="text"
                    required
                    maxLength={11}
                    placeholder="ej. 30714455660"
                    value={nuevoCliente.cuit}
                    onChange={(e) => setNuevoCliente({ ...nuevoCliente, cuit: e.target.value })}
                  />
                </div>

                <div className="form-group">
                  <label>Razón Social:</label>
                  <input
                    type="text"
                    required
                    placeholder="ej. Empresa S.A."
                    value={nuevoCliente.razonSocial}
                    onChange={(e) => setNuevoCliente({ ...nuevoCliente, razonSocial: e.target.value })}
                  />
                </div>

                <div className="form-group">
                  <label>Nombre de Fantasía (opcional):</label>
                  <input
                    type="text"
                    value={nuevoCliente.nombreFantasia}
                    onChange={(e) => setNuevoCliente({ ...nuevoCliente, nombreFantasia: e.target.value })}
                  />
                </div>

                <div className="form-group">
                  <label>Tipo de Persona:</label>
                  <select
                    value={nuevoCliente.tipoPersona}
                    onChange={(e) =>
                      setNuevoCliente({ ...nuevoCliente, tipoPersona: e.target.value as TipoPersona })
                    }
                  >
                    <option value="Fisica">Persona Humana / Física</option>
                    <option value="Juridica">Persona Jurídica (S.A., S.R.L., etc.)</option>
                  </select>
                </div>

                <div className="form-group">
                  <label>Régimen Tributario:</label>
                  <select
                    value={nuevoCliente.regimenTributario}
                    onChange={(e) =>
                      setNuevoCliente({
                        ...nuevoCliente,
                        regimenTributario: e.target.value as RegimenTributario,
                      })
                    }
                  >
                    <option value="ResponsableInscripto">Responsable Inscripto</option>
                    <option value="Monotributo">Monotributo</option>
                    <option value="Exento">Exento</option>
                  </select>
                </div>

                <div className="form-group">
                  <label>Email de Contacto:</label>
                  <input
                    type="email"
                    placeholder="administracion@cliente.com"
                    value={nuevoCliente.emailContacto}
                    onChange={(e) => setNuevoCliente({ ...nuevoCliente, emailContacto: e.target.value })}
                  />
                </div>
              </div>

              <div className="form-group mt-3">
                <label>Impuestos / Obligaciones Iniciales:</label>
                <div className="checkbox-grid">
                  {[
                    { id: 'IVA', label: 'IVA (F.2002)' },
                    { id: 'F931_LSD', label: 'Cargas Sociales F.931 (LSD)' },
                    { id: 'MONOTRIBUTO', label: 'Monotributo Mensual' },
                    { id: 'IIBB_CM', label: 'Convenio Multilateral (CM03)' },
                    { id: 'IIBB_LOCAL', label: 'IIBB Local (ARBA / AGIP)' },
                    { id: 'GANANCIAS_PJ', label: 'Ganancias Sociedades' },
                  ].map((imp) => (
                    <label key={imp.id} className="checkbox-label">
                      <input
                        type="checkbox"
                        checked={nuevoCliente.obligaciones.includes(imp.id)}
                        onChange={() => toggleObligacion(imp.id)}
                      />
                      <span>{imp.label}</span>
                    </label>
                  ))}
                </div>
              </div>

              <div className="modal-actions">
                <button type="button" className="btn btn-secondary" onClick={() => setModalNuevo(false)}>
                  Cancelar
                </button>
                <button type="submit" className="btn btn-primary">
                  Guardar Cliente
                </button>
              </div>
            </form>
          </div>
        </div>
      )}
    </div>
  );
};

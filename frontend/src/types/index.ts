export type RolUsuario = 'AdminEstudio' | 'Colaborador' | 'ClientePortal';

export type EstadoVencimiento = 'Pendiente' | 'EnProceso' | 'Presentado' | 'Pagado' | 'Vencido';

export type EstadoTarea = 'Pendiente' | 'EnProceso' | 'Completada' | 'Cancelada';

export type PrioridadTarea = 'Baja' | 'Media' | 'Alta' | 'Urgente';

export type TipoPersona = 'Fisica' | 'Juridica';

export type RegimenTributario = 'Monotributo' | 'ResponsableInscripto' | 'Exento' | 'ConsumidorFinal';

export type Periodicidad = 'Mensual' | 'Quincenal' | 'Semestral' | 'Anual';

export type CategoriaDocumento = 
  | 'AcusePresentacion'
  | 'ComprobantePago_VEP'
  | 'FacturaVentas'
  | 'FacturaCompras'
  | 'ExtractoBancario'
  | 'ConstanciaInscripcion'
  | 'Contrato'
  | 'Otro';

export interface UsuarioSession {
  id: string;
  nombreCompleto: string;
  email: string;
  rol: RolUsuario;
  estudioId: string;
  nombreEstudio: string;
  clienteId?: string | null;
}

export interface ClienteListDto {
  id: string;
  cuit: string;
  razonSocial: string;
  nombreFantasia?: string;
  tipoPersona: TipoPersona;
  regimenTributario: RegimenTributario;
  emailContacto?: string;
  telefonoContacto?: string;
  responsableNombre?: string;
  cantidadObligaciones: number;
  activo: boolean;
}

export interface ClienteObligacionDto {
  id: string;
  obligacionId: string;
  nombre: string;
  organismo: string;
  periodicidad: Periodicidad;
  jurisdiccion?: string;
  diaVencimientoPersonalizado?: number;
  activo: boolean;
}

export interface ClienteDetailDto {
  id: string;
  cuit: string;
  razonSocial: string;
  nombreFantasia?: string;
  tipoPersona: TipoPersona;
  regimenTributario: RegimenTributario;
  cierreEjercicioMes?: number;
  emailContacto?: string;
  telefonoContacto?: string;
  responsableAsignadoId?: string;
  responsableNombre?: string;
  notas?: string;
  activo: boolean;
  obligaciones: ClienteObligacionDto[];
}

export interface VencimientoDto {
  id: string;
  clienteId: string;
  clienteRazonSocial: string;
  clienteCuit: string;
  clienteObligacionId: string;
  obligacionId: string;
  obligacionNombre: string;
  jurisdiccion?: string;
  periodoFiscal: string;
  fechaVencimientoOriginal: string;
  fechaVencimientoEfectiva: string;
  tieneProrroga: boolean;
  motivoProrroga?: string;
  estado: EstadoVencimiento;
  montoAPagar?: number;
  fechaPresentacion?: string;
  fechaPago?: string;
  responsableId?: string;
  responsableNombre?: string;
  totalTareas: number;
  tareasCompletadas: number;
  totalDocumentos: number;
  notas?: string;
}

export interface TareaDto {
  id: string;
  clienteId: string;
  clienteRazonSocial: string;
  vencimientoId?: string;
  vencimientoInfo?: string;
  titulo: string;
  descripcion?: string;
  asignadoAUsuarioId?: string;
  asignadoANombre?: string;
  estado: EstadoTarea;
  prioridad: PrioridadTarea;
  fechaLimite?: string;
  orden: number;
  createdAt: string;
}

export interface DocumentoDto {
  id: string;
  clienteId: string;
  clienteRazonSocial: string;
  vencimientoId?: string;
  vencimientoInfo?: string;
  nombreOriginal: string;
  contentType: string;
  tamanoBytes: number;
  categoria: CategoriaDocumento;
  visibleParaCliente: boolean;
  subidoPorUsuarioId: string;
  subidoPorNombre: string;
  createdAt: string;
}

export interface DashboardResumenDto {
  totalClientesActivos: number;
  vencimientosMesTotal: number;
  vencimientosPresentados: number;
  vencimientosPendientes: number;
  vencimientosVencidos: number;
  tareasPendientes: number;
  proximosVencimientos: VencimientoDto[];
  vencimientosUrgentes: VencimientoDto[];
  misTareasPendientes: TareaDto[];
}

export interface PortalResumenDto {
  razonSocial: string;
  cuit: string;
  regimenTributario: string;
  periodoActual: string;
  tramitesEnProceso: number;
  tramitesPresentados: number;
  tramitesPagados: number;
  totalMontoPendientePago: number;
  vencimientosDelMes: PortalVencimientoDto[];
}

export interface PortalVencimientoDto {
  id: string;
  impuesto: string;
  periodoFiscal: string;
  fechaVencimiento: string;
  estado: EstadoVencimiento;
  montoAPagar?: number;
  fechaPresentacion?: string;
  fechaPago?: string;
  documentosDisponibles: PortalDocumentoDto[];
}

export interface PortalDocumentoDto {
  id: string;
  titulo: string;
  nombreArchivo: string;
  categoria: string;
  tamanoBytes: number;
  fechaSubida: string;
}

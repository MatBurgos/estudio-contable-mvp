namespace EstudioContable.Domain.Enums;

public enum RolUsuario
{
    AdminEstudio = 1,
    Colaborador = 2,
    ClientePortal = 3
}

public enum EstadoVencimiento
{
    Pendiente = 1,
    EnProceso = 2,
    Presentado = 3,
    Pagado = 4,
    Vencido = 5
}

public enum EstadoTarea
{
    Pendiente = 1,
    EnProceso = 2,
    Completada = 3,
    Cancelada = 4
}

public enum PrioridadTarea
{
    Baja = 1,
    Media = 2,
    Alta = 3,
    Urgente = 4
}

public enum TipoPersona
{
    Fisica = 1,
    Juridica = 2
}

public enum RegimenTributario
{
    Monotributo = 1,
    ResponsableInscripto = 2,
    Exento = 3,
    ConsumidorFinal = 4
}

public enum Periodicidad
{
    Mensual = 1,
    Quincenal = 2,
    Semestral = 3,
    Anual = 4
}

public enum CategoriaDocumento
{
    AcusePresentacion = 1,
    ComprobantePago_VEP = 2,
    FacturaVentas = 3,
    FacturaCompras = 4,
    ExtractoBancario = 5,
    ConstanciaInscripcion = 6,
    Contrato = 7,
    Otro = 99
}

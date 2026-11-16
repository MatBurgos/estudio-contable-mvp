using EstudioContable.Domain.Enums;

namespace EstudioContable.Application.DTOs.Clientes;

public record ClienteListDto(
    Guid Id,
    string Cuit,
    string RazonSocial,
    string? NombreFantasia,
    TipoPersona TipoPersona,
    RegimenTributario RegimenTributario,
    string? EmailContacto,
    string? TelefonoContacto,
    string? ResponsableNombre,
    int CantidadObligaciones,
    bool Activo
);

public record ClienteDetailDto(
    Guid Id,
    string Cuit,
    string RazonSocial,
    string? NombreFantasia,
    TipoPersona TipoPersona,
    RegimenTributario RegimenTributario,
    int? CierreEjercicioMes,
    string? EmailContacto,
    string? TelefonoContacto,
    Guid? ResponsableAsignadoId,
    string? ResponsableNombre,
    string? Notas,
    bool Activo,
    List<ClienteObligacionDto> Obligaciones
);

public record CreateClienteDto(
    string Cuit,
    string RazonSocial,
    string? NombreFantasia,
    TipoPersona TipoPersona,
    RegimenTributario RegimenTributario,
    int? CierreEjercicioMes,
    string? EmailContacto,
    string? TelefonoContacto,
    Guid? ResponsableAsignadoId,
    string? Notas,
    List<string>? ObligacionesIdsIniciales
);

public record UpdateClienteDto(
    string RazonSocial,
    string? NombreFantasia,
    TipoPersona TipoPersona,
    RegimenTributario RegimenTributario,
    int? CierreEjercicioMes,
    string? EmailContacto,
    string? TelefonoContacto,
    Guid? ResponsableAsignadoId,
    string? Notas
);

public record ClienteObligacionDto(
    Guid Id,
    string ObligacionId,
    string Nombre,
    string Organismo,
    Periodicidad Periodicidad,
    string? Jurisdiccion,
    int? DiaVencimientoPersonalizado,
    bool Activo
);

public record AsignarObligacionDto(
    string ObligacionId,
    string? Jurisdiccion,
    int? DiaVencimientoPersonalizado
);

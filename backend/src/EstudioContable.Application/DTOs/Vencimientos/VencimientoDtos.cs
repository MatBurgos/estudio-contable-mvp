using EstudioContable.Domain.Enums;

namespace EstudioContable.Application.DTOs.Vencimientos;

public record VencimientoDto(
    Guid Id,
    Guid ClienteId,
    string ClienteRazonSocial,
    string ClienteCuit,
    Guid ClienteObligacionId,
    string ObligacionId,
    string ObligacionNombre,
    string? Jurisdiccion,
    string PeriodoFiscal,
    DateOnly FechaVencimientoOriginal,
    DateOnly FechaVencimientoEfectiva,
    bool TieneProrroga,
    string? MotivoProrroga,
    EstadoVencimiento Estado,
    decimal? MontoAPagar,
    DateTime? FechaPresentacion,
    DateTime? FechaPago,
    Guid? ResponsableId,
    string? ResponsableNombre,
    int TotalTareas,
    int TareasCompletadas,
    int TotalDocumentos,
    string? Notas
);

public record ProrrogaVencimientoDto(
    DateOnly NuevaFechaEfectiva,
    string Motivo
);

public record UpdateEstadoVencimientoDto(
    EstadoVencimiento NuevoEstado,
    decimal? MontoAPagar,
    DateTime? FechaAccion
);

public record GenerarPeriodoDto(
    string PeriodoFiscal, // ej: "2026-09"
    Guid? ClienteId       // si es null, genera para todos los clientes activos del estudio
);

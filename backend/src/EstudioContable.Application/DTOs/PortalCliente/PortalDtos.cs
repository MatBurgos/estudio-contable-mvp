using EstudioContable.Domain.Enums;

namespace EstudioContable.Application.DTOs.PortalCliente;

public record PortalResumenDto(
    string RazonSocial,
    string Cuit,
    string RegimenTributario,
    string PeriodoActual,
    int TramitesEnProceso,
    int TramitesPresentados,
    int TramitesPagados,
    decimal TotalMontoPendientePago,
    List<PortalVencimientoDto> VencimientosDelMes
);

public record PortalVencimientoDto(
    Guid Id,
    string Impuesto,
    string PeriodoFiscal,
    DateOnly FechaVencimiento,
    EstadoVencimiento Estado,
    decimal? MontoAPagar,
    DateTime? FechaPresentacion,
    DateTime? FechaPago,
    List<PortalDocumentoDto> DocumentosDisponibles
);

public record PortalDocumentoDto(
    Guid Id,
    string Titulo,
    string NombreArchivo,
    string Categoria,
    long TamanoBytes,
    DateTime FechaSubida
);

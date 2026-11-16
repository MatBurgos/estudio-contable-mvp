using EstudioContable.Application.DTOs.Vencimientos;
using EstudioContable.Domain.Enums;

namespace EstudioContable.Application.Services;

public interface IVencimientoService
{
    Task<List<VencimientoDto>> GetVencimientosAsync(
        string? periodo = null,
        Guid? clienteId = null,
        EstadoVencimiento? estado = null,
        string? obligacionId = null,
        Guid? responsableId = null,
        CancellationToken cancellationToken = default);

    Task<VencimientoDto?> GetVencimientoByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<VencimientoDto?> AplicarProrrogaAsync(Guid id, ProrrogaVencimientoDto dto, CancellationToken cancellationToken = default);
    Task<VencimientoDto?> ActualizarEstadoAsync(Guid id, UpdateEstadoVencimientoDto dto, CancellationToken cancellationToken = default);
    Task<int> GenerarVencimientosPeriodoAsync(GenerarPeriodoDto dto, CancellationToken cancellationToken = default);
}

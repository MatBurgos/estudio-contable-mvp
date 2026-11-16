using EstudioContable.Application.DTOs.Tareas;
using EstudioContable.Domain.Enums;

namespace EstudioContable.Application.Services;

public interface ITareaService
{
    Task<List<TareaDto>> GetTareasAsync(
        Guid? clienteId = null,
        Guid? vencimientoId = null,
        Guid? asignadoAId = null,
        EstadoTarea? estado = null,
        CancellationToken cancellationToken = default);

    Task<TareaDto?> GetTareaByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<TareaDto> CreateTareaAsync(CreateTareaDto dto, CancellationToken cancellationToken = default);
    Task<TareaDto?> UpdateTareaAsync(Guid id, UpdateTareaDto dto, CancellationToken cancellationToken = default);
    Task<TareaDto?> UpdateEstadoAsync(Guid id, UpdateEstadoTareaDto dto, CancellationToken cancellationToken = default);
    Task<bool> DeleteTareaAsync(Guid id, CancellationToken cancellationToken = default);
}

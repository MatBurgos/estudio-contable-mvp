using EstudioContable.Domain.Enums;

namespace EstudioContable.Application.DTOs.Tareas;

public record TareaDto(
    Guid Id,
    Guid ClienteId,
    string ClienteRazonSocial,
    Guid? VencimientoId,
    string? VencimientoInfo,
    string Titulo,
    string? Descripcion,
    Guid? AsignadoAUsuarioId,
    string? AsignadoANombre,
    EstadoTarea Estado,
    PrioridadTarea Prioridad,
    DateOnly? FechaLimite,
    int Orden,
    DateTime CreatedAt
);

public record CreateTareaDto(
    Guid ClienteId,
    Guid? VencimientoId,
    string Titulo,
    string? Descripcion,
    Guid? AsignadoAUsuarioId,
    PrioridadTarea Prioridad,
    DateOnly? FechaLimite
);

public record UpdateTareaDto(
    string Titulo,
    string? Descripcion,
    Guid? AsignadoAUsuarioId,
    EstadoTarea Estado,
    PrioridadTarea Prioridad,
    DateOnly? FechaLimite,
    int Orden
);

public record UpdateEstadoTareaDto(
    EstadoTarea NuevoEstado
);

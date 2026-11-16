using EstudioContable.Domain.Common;
using EstudioContable.Domain.Enums;

namespace EstudioContable.Domain.Entities;

/// <summary>
/// Tarea operativa del equipo contable. Puede pertenecer a un vencimiento
/// (ej. "Pedir extractos bancarios", "Cargar comprobantes", "Enviar VEP")
/// o ser una tarea libre asignada a un cliente.
/// </summary>
public class Tarea : BaseEntity, ITenantEntity
{
    public Guid EstudioId { get; set; }
    public Estudio Estudio { get; set; } = null!;

    public Guid ClienteId { get; set; }
    public Cliente Cliente { get; set; } = null!;

    public Guid? VencimientoId { get; set; }
    public Vencimiento? Vencimiento { get; set; }

    public string Titulo { get; set; } = string.Empty;
    public string? Descripcion { get; set; }

    public Guid? AsignadoAUsuarioId { get; set; }
    public Usuario? AsignadoA { get; set; }

    public EstadoTarea Estado { get; set; } = EstadoTarea.Pendiente;
    public PrioridadTarea Prioridad { get; set; } = PrioridadTarea.Media;

    public DateOnly? FechaLimite { get; set; }
    public int Orden { get; set; } = 0;
}

using EstudioContable.Domain.Common;
using EstudioContable.Domain.Enums;

namespace EstudioContable.Domain.Entities;

/// <summary>
/// Representa el vencimiento concreto de un impuesto para un cliente en un período fiscal.
/// Permite registrar prórrogas oficiales o internas sin perder la fecha original.
/// </summary>
public class Vencimiento : BaseEntity, ITenantEntity
{
    public Guid EstudioId { get; set; }
    public Estudio Estudio { get; set; } = null!;

    public Guid ClienteId { get; set; }
    public Cliente Cliente { get; set; } = null!;

    public Guid ClienteObligacionId { get; set; }
    public ClienteObligacion ClienteObligacion { get; set; } = null!;

    /// <summary>
    /// Formato "YYYY-MM" (ej. "2026-09") o "YYYY" para anuales.
    /// </summary>
    public string PeriodoFiscal { get; set; } = string.Empty;

    /// <summary>
    /// Fecha original según el calendario de AFIP/ARCA o el organismo recaudador.
    /// </summary>
    public DateOnly FechaVencimientoOriginal { get; set; }

    /// <summary>
    /// Fecha operativa efectiva: se actualiza si hay prórroga oficial o reprogramación interna.
    /// </summary>
    public DateOnly FechaVencimientoEfectiva { get; set; }

    public bool TieneProrroga { get; set; } = false;
    public string? MotivoProrroga { get; set; }

    public EstadoVencimiento Estado { get; set; } = EstadoVencimiento.Pendiente;

    /// <summary>
    /// Monto a pagar resultante de la liquidación / VEP (opcional).
    /// </summary>
    public decimal? MontoAPagar { get; set; }

    public DateTime? FechaPresentacion { get; set; }
    public DateTime? FechaPago { get; set; }

    public Guid? ResponsableId { get; set; }
    public Usuario? Responsable { get; set; }

    public string? Notas { get; set; }

    // Relaciones
    public ICollection<Tarea> Tareas { get; set; } = new List<Tarea>();
    public ICollection<Documento> Documentos { get; set; } = new List<Documento>();
}

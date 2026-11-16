using EstudioContable.Domain.Common;

namespace EstudioContable.Domain.Entities;

/// <summary>
/// Vincula un Cliente con una Obligación impositiva activa en el estudio.
/// </summary>
public class ClienteObligacion : BaseEntity, ITenantEntity
{
    public Guid EstudioId { get; set; }
    public Estudio Estudio { get; set; } = null!;

    public Guid ClienteId { get; set; }
    public Cliente Cliente { get; set; } = null!;

    public string ObligacionId { get; set; } = string.Empty;
    public Obligacion Obligacion { get; set; } = null!;

    /// <summary>
    /// Relevante para Ingresos Brutos (ej: "901 - CABA", "902 - Buenos Aires").
    /// </summary>
    public string? Jurisdiccion { get; set; }

    /// <summary>
    /// Si el estudio desea fijar un día de vencimiento personalizado (override del calendario general).
    /// </summary>
    public int? DiaVencimientoPersonalizado { get; set; }

    public bool Activo { get; set; } = true;
    public string? Notas { get; set; }

    public ICollection<Vencimiento> Vencimientos { get; set; } = new List<Vencimiento>();
}

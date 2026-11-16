using EstudioContable.Domain.Enums;

namespace EstudioContable.Domain.Entities;

/// <summary>
/// Catálogo maestro de tributos y regímenes en Argentina (IVA, IIBB, F931, Monotributo, etc.).
/// </summary>
public class Obligacion
{
    public string Id { get; set; } = string.Empty; // ej: "IVA", "IIBB_CM", "F931", "MONOTRIBUTO"
    public string Nombre { get; set; } = string.Empty;
    public string Organismo { get; set; } = string.Empty; // "ARCA/AFIP", "COMARB", "RENTAS"
    public Periodicidad Periodicidad { get; set; } = Periodicidad.Mensual;
    public bool RequiereJurisdiccion { get; set; } = false;
    public bool EsFrecuente { get; set; } = true;
    public string? Descripcion { get; set; }

    public ICollection<ClienteObligacion> ClientesInscriptos { get; set; } = new List<ClienteObligacion>();
}

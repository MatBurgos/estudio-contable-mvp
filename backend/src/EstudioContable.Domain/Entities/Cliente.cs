using EstudioContable.Domain.Common;
using EstudioContable.Domain.Enums;

namespace EstudioContable.Domain.Entities;

public class Cliente : BaseEntity, ITenantEntity
{
    public Guid EstudioId { get; set; }
    public Estudio Estudio { get; set; } = null!;

    /// <summary>
    /// CUIT argentino de 11 dígitos sin guiones (ej. 20301234567).
    /// El último dígito determina el calendario de vencimiento de AFIP/ARCA.
    /// </summary>
    public string Cuit { get; set; } = string.Empty;
    public string RazonSocial { get; set; } = string.Empty;
    public string? NombreFantasia { get; set; }
    public TipoPersona TipoPersona { get; set; } = TipoPersona.Fisica;
    public RegimenTributario RegimenTributario { get; set; } = RegimenTributario.Monotributo;

    /// <summary>
    /// Mes de cierre de ejercicio (1 a 12), relevante para personas jurídicas y cálculo de Ganancias PJ.
    /// </summary>
    public int? CierreEjercicioMes { get; set; }

    public string? EmailContacto { get; set; }
    public string? TelefonoContacto { get; set; }

    /// <summary>
    /// Profesional del estudio asignado como responsable principal.
    /// </summary>
    public Guid? ResponsableAsignadoId { get; set; }
    public Usuario? ResponsableAsignado { get; set; }

    public string? Notas { get; set; }
    public bool Activo { get; set; } = true;

    // Relaciones
    public ICollection<ClienteObligacion> Obligaciones { get; set; } = new List<ClienteObligacion>();
    public ICollection<Vencimiento> Vencimientos { get; set; } = new List<Vencimiento>();
    public ICollection<Tarea> Tareas { get; set; } = new List<Tarea>();
    public ICollection<Documento> Documentos { get; set; } = new List<Documento>();
    public ICollection<Usuario> UsuariosPortal { get; set; } = new List<Usuario>();

    /// <summary>
    /// Helper para obtener el último dígito del CUIT para el calendario impositivo.
    /// </summary>
    public int UltimoDigitoCuit => !string.IsNullOrEmpty(Cuit) && char.IsDigit(Cuit[^1]) 
        ? Cuit[^1] - '0' 
        : 0;
}

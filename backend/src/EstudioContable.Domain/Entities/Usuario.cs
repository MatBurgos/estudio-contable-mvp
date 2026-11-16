using EstudioContable.Domain.Common;
using EstudioContable.Domain.Enums;

namespace EstudioContable.Domain.Entities;

public class Usuario : BaseEntity, ITenantEntity
{
    public Guid EstudioId { get; set; }
    public Estudio Estudio { get; set; } = null!;

    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public string Nombre { get; set; } = string.Empty;
    public string Apellido { get; set; } = string.Empty;
    public RolUsuario Rol { get; set; } = RolUsuario.Colaborador;

    /// <summary>
    /// Si el rol es ClientePortal, este campo vincula al cliente específico de solo lectura.
    /// Si es Admin o Colaborador, es null.
    /// </summary>
    public Guid? ClienteId { get; set; }
    public Cliente? Cliente { get; set; }

    public bool Activo { get; set; } = true;
    public DateTime? UltimoAccesoAt { get; set; }

    // Relaciones
    public ICollection<Tarea> TareasAsignadas { get; set; } = new List<Tarea>();
    public ICollection<Cliente> ClientesACargo { get; set; } = new List<Cliente>();
    public ICollection<Documento> DocumentosSubidos { get; set; } = new List<Documento>();
}

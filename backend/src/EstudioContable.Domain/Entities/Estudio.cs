using EstudioContable.Domain.Common;

namespace EstudioContable.Domain.Entities;

public class Estudio : BaseEntity
{
    public string Nombre { get; set; } = string.Empty;
    public string Cuit { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? Telefono { get; set; }
    public string Plan { get; set; } = "Trial";
    public bool Activo { get; set; } = true;

    // Relaciones
    public ICollection<Usuario> Usuarios { get; set; } = new List<Usuario>();
    public ICollection<Cliente> Clientes { get; set; } = new List<Cliente>();
    public ICollection<Vencimiento> Vencimientos { get; set; } = new List<Vencimiento>();
    public ICollection<Tarea> Tareas { get; set; } = new List<Tarea>();
    public ICollection<Documento> Documentos { get; set; } = new List<Documento>();
}

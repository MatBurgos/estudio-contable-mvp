using Microsoft.EntityFrameworkCore;
using EstudioContable.Domain.Entities;

namespace EstudioContable.Application.Common.Interfaces;

public interface IAppDbContext
{
    DbSet<Estudio> Estudios { get; }
    DbSet<Usuario> Usuarios { get; }
    DbSet<Cliente> Clientes { get; }
    DbSet<Obligacion> Obligaciones { get; }
    DbSet<ClienteObligacion> ClienteObligaciones { get; }
    DbSet<Vencimiento> Vencimientos { get; }
    DbSet<Tarea> Tareas { get; }
    DbSet<Documento> Documentos { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}

using Microsoft.EntityFrameworkCore;
using EstudioContable.Application.Common.Interfaces;
using EstudioContable.Domain.Common;
using EstudioContable.Domain.Entities;

namespace EstudioContable.Infrastructure.Persistence;

public class AppDbContext : DbContext, IAppDbContext
{
    private readonly ICurrentTenantService _tenantService;

    public AppDbContext(DbContextOptions<AppDbContext> options, ICurrentTenantService tenantService)
        : base(options)
    {
        _tenantService = tenantService;
    }

    public DbSet<Estudio> Estudios => Set<Estudio>();
    public DbSet<Usuario> Usuarios => Set<Usuario>();
    public DbSet<Cliente> Clientes => Set<Cliente>();
    public DbSet<Obligacion> Obligaciones => Set<Obligacion>();
    public DbSet<ClienteObligacion> ClienteObligaciones => Set<ClienteObligacion>();
    public DbSet<Vencimiento> Vencimientos => Set<Vencimiento>();
    public DbSet<Tarea> Tareas => Set<Tarea>();
    public DbSet<Documento> Documentos => Set<Documento>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // ==========================================
        // 1. GLOBAL QUERY FILTERS (Multi-tenancy)
        // ==========================================
        // Si el usuario pertenece a un estudio, solo ve datos de su EstudioId.
        // Si no hay tenant autenticado (ej. migraciones, seeding), no filtra.
        modelBuilder.Entity<Usuario>().HasQueryFilter(e => !_tenantService.CurrentEstudioId.HasValue || e.EstudioId == _tenantService.CurrentEstudioId.Value);
        modelBuilder.Entity<Cliente>().HasQueryFilter(e => !_tenantService.CurrentEstudioId.HasValue || e.EstudioId == _tenantService.CurrentEstudioId.Value);
        modelBuilder.Entity<ClienteObligacion>().HasQueryFilter(e => !_tenantService.CurrentEstudioId.HasValue || e.EstudioId == _tenantService.CurrentEstudioId.Value);
        modelBuilder.Entity<Vencimiento>().HasQueryFilter(e => !_tenantService.CurrentEstudioId.HasValue || e.EstudioId == _tenantService.CurrentEstudioId.Value);
        modelBuilder.Entity<Tarea>().HasQueryFilter(e => !_tenantService.CurrentEstudioId.HasValue || e.EstudioId == _tenantService.CurrentEstudioId.Value);
        modelBuilder.Entity<Documento>().HasQueryFilter(e => !_tenantService.CurrentEstudioId.HasValue || e.EstudioId == _tenantService.CurrentEstudioId.Value);

        // ==========================================
        // 2. CONFIGURACIÓN DE TABLAS Y RELACIONES
        // ==========================================
        // Estudio
        modelBuilder.Entity<Estudio>(b =>
        {
            b.ToTable("Estudios");
            b.HasKey(e => e.Id);
            b.Property(e => e.Nombre).IsRequired().HasMaxLength(150);
            b.Property(e => e.Cuit).IsRequired().HasMaxLength(11);
            b.Property(e => e.Email).IsRequired().HasMaxLength(150);
            b.Property(e => e.Plan).HasMaxLength(30).HasDefaultValue("Trial");
            b.HasIndex(e => e.Cuit);
        });

        // Usuario
        modelBuilder.Entity<Usuario>(b =>
        {
            b.ToTable("Usuarios");
            b.HasKey(u => u.Id);
            b.Property(u => u.Email).IsRequired().HasMaxLength(150);
            b.Property(u => u.Nombre).IsRequired().HasMaxLength(100);
            b.Property(u => u.Apellido).IsRequired().HasMaxLength(100);
            b.Property(u => u.Rol).HasConversion<string>().HasMaxLength(30);

            b.HasOne(u => u.Estudio)
                .WithMany(e => e.Usuarios)
                .HasForeignKey(u => u.EstudioId)
                .OnDelete(DeleteBehavior.Restrict);

            b.HasOne(u => u.Cliente)
                .WithMany(c => c.UsuariosPortal)
                .HasForeignKey(u => u.ClienteId)
                .OnDelete(DeleteBehavior.SetNull);

            b.HasIndex(u => new { u.EstudioId, u.Email }).IsUnique();
        });

        // Cliente
        modelBuilder.Entity<Cliente>(b =>
        {
            b.ToTable("Clientes");
            b.HasKey(c => c.Id);
            b.Property(c => c.Cuit).IsRequired().HasMaxLength(11);
            b.Property(c => c.RazonSocial).IsRequired().HasMaxLength(200);
            b.Property(c => c.NombreFantasia).HasMaxLength(150);
            b.Property(c => c.TipoPersona).HasConversion<string>().HasMaxLength(20);
            b.Property(c => c.RegimenTributario).HasConversion<string>().HasMaxLength(30);

            b.HasOne(c => c.Estudio)
                .WithMany(e => e.Clientes)
                .HasForeignKey(c => c.EstudioId)
                .OnDelete(DeleteBehavior.Restrict);

            b.HasOne(c => c.ResponsableAsignado)
                .WithMany(u => u.ClientesACargo)
                .HasForeignKey(c => c.ResponsableAsignadoId)
                .OnDelete(DeleteBehavior.SetNull);

            b.HasIndex(c => new { c.EstudioId, c.Cuit });
            b.HasIndex(c => new { c.EstudioId, c.Activo });
        });

        // Obligación (Catálogo global)
        modelBuilder.Entity<Obligacion>(b =>
        {
            b.ToTable("Obligaciones");
            b.HasKey(o => o.Id);
            b.Property(o => o.Id).HasMaxLength(50);
            b.Property(o => o.Nombre).IsRequired().HasMaxLength(150);
            b.Property(o => o.Organismo).IsRequired().HasMaxLength(50);
            b.Property(o => o.Periodicidad).HasConversion<string>().HasMaxLength(20);
        });

        // ClienteObligacion
        modelBuilder.Entity<ClienteObligacion>(b =>
        {
            b.ToTable("ClienteObligaciones");
            b.HasKey(co => co.Id);
            b.Property(co => co.ObligacionId).IsRequired().HasMaxLength(50);
            b.Property(co => co.Jurisdiccion).HasMaxLength(100);

            b.HasOne(co => co.Estudio)
                .WithMany()
                .HasForeignKey(co => co.EstudioId)
                .OnDelete(DeleteBehavior.Restrict);

            b.HasOne(co => co.Cliente)
                .WithMany(c => c.Obligaciones)
                .HasForeignKey(co => co.ClienteId)
                .OnDelete(DeleteBehavior.Cascade);

            b.HasOne(co => co.Obligacion)
                .WithMany(o => o.ClientesInscriptos)
                .HasForeignKey(co => co.ObligacionId)
                .OnDelete(DeleteBehavior.Restrict);

            b.HasIndex(co => new { co.ClienteId, co.ObligacionId });
        });

        // Vencimiento
        modelBuilder.Entity<Vencimiento>(b =>
        {
            b.ToTable("Vencimientos");
            b.HasKey(v => v.Id);
            b.Property(v => v.PeriodoFiscal).IsRequired().HasMaxLength(7);
            b.Property(v => v.Estado).HasConversion<string>().HasMaxLength(20);
            b.Property(v => v.MontoAPagar).HasColumnType("numeric(14,2)");

            b.HasOne(v => v.Estudio)
                .WithMany(e => e.Vencimientos)
                .HasForeignKey(v => v.EstudioId)
                .OnDelete(DeleteBehavior.Restrict);

            b.HasOne(v => v.Cliente)
                .WithMany(c => c.Vencimientos)
                .HasForeignKey(v => v.ClienteId)
                .OnDelete(DeleteBehavior.Cascade);

            b.HasOne(v => v.ClienteObligacion)
                .WithMany(co => co.Vencimientos)
                .HasForeignKey(v => v.ClienteObligacionId)
                .OnDelete(DeleteBehavior.Cascade);

            b.HasOne(v => v.Responsable)
                .WithMany()
                .HasForeignKey(v => v.ResponsableId)
                .OnDelete(DeleteBehavior.SetNull);

            b.HasIndex(v => new { v.EstudioId, v.PeriodoFiscal });
            b.HasIndex(v => new { v.EstudioId, v.FechaVencimientoEfectiva });
            b.HasIndex(v => new { v.EstudioId, v.Estado });
        });

        // Tarea
        modelBuilder.Entity<Tarea>(b =>
        {
            b.ToTable("Tareas");
            b.HasKey(t => t.Id);
            b.Property(t => t.Titulo).IsRequired().HasMaxLength(200);
            b.Property(t => t.Estado).HasConversion<string>().HasMaxLength(20);
            b.Property(t => t.Prioridad).HasConversion<string>().HasMaxLength(15);

            b.HasOne(t => t.Estudio)
                .WithMany(e => e.Tareas)
                .HasForeignKey(t => t.EstudioId)
                .OnDelete(DeleteBehavior.Restrict);

            b.HasOne(t => t.Cliente)
                .WithMany(c => c.Tareas)
                .HasForeignKey(t => t.ClienteId)
                .OnDelete(DeleteBehavior.Cascade);

            b.HasOne(t => t.Vencimiento)
                .WithMany(v => v.Tareas)
                .HasForeignKey(t => t.VencimientoId)
                .OnDelete(DeleteBehavior.Cascade);

            b.HasOne(t => t.AsignadoA)
                .WithMany(u => u.TareasAsignadas)
                .HasForeignKey(t => t.AsignadoAUsuarioId)
                .OnDelete(DeleteBehavior.SetNull);

            b.HasIndex(t => new { t.EstudioId, t.Estado });
            b.HasIndex(t => new { t.EstudioId, t.AsignadoAUsuarioId });
        });

        // Documento
        modelBuilder.Entity<Documento>(b =>
        {
            b.ToTable("Documentos");
            b.HasKey(d => d.Id);
            b.Property(d => d.NombreOriginal).IsRequired().HasMaxLength(255);
            b.Property(d => d.NombreAlmacenado).IsRequired().HasMaxLength(255);
            b.Property(d => d.RutaAlmacenamiento).IsRequired().HasMaxLength(500);
            b.Property(d => d.ContentType).IsRequired().HasMaxLength(100);
            b.Property(d => d.Categoria).HasConversion<string>().HasMaxLength(30);

            b.HasOne(d => d.Estudio)
                .WithMany(e => e.Documentos)
                .HasForeignKey(d => d.EstudioId)
                .OnDelete(DeleteBehavior.Restrict);

            b.HasOne(d => d.Cliente)
                .WithMany(c => c.Documentos)
                .HasForeignKey(d => d.ClienteId)
                .OnDelete(DeleteBehavior.Cascade);

            b.HasOne(d => d.Vencimiento)
                .WithMany(v => v.Documentos)
                .HasForeignKey(d => d.VencimientoId)
                .OnDelete(DeleteBehavior.SetNull);

            b.HasOne(d => d.SubidoPor)
                .WithMany(u => u.DocumentosSubidos)
                .HasForeignKey(d => d.SubidoPorUsuarioId)
                .OnDelete(DeleteBehavior.Restrict);

            b.HasIndex(d => new { d.EstudioId, d.ClienteId });
            b.HasIndex(d => new { d.EstudioId, d.VencimientoId });
        });
    }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        var tenantId = _tenantService.CurrentEstudioId;

        foreach (var entry in ChangeTracker.Entries())
        {
            // Auto inyección de EstudioId en entidades multi-tenant
            if (entry.Entity is ITenantEntity tenantEntity && entry.State == EntityState.Added)
            {
                if (tenantEntity.EstudioId == Guid.Empty && tenantId.HasValue)
                {
                    tenantEntity.EstudioId = tenantId.Value;
                }
            }

            // Timestamps automáticos
            if (entry.Entity is BaseEntity baseEntity)
            {
                if (entry.State == EntityState.Added && baseEntity.CreatedAt == default)
                {
                    baseEntity.CreatedAt = DateTime.UtcNow;
                }
                else if (entry.State == EntityState.Modified)
                {
                    baseEntity.UpdatedAt = DateTime.UtcNow;
                }
            }
        }

        return base.SaveChangesAsync(cancellationToken);
    }
}

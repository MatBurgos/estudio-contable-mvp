using Microsoft.EntityFrameworkCore;
using EstudioContable.Application.Common.Interfaces;
using EstudioContable.Application.DTOs.Vencimientos;
using EstudioContable.Domain.Entities;
using EstudioContable.Domain.Enums;

namespace EstudioContable.Application.Services;

public class VencimientoService : IVencimientoService
{
    private readonly IAppDbContext _context;
    private readonly ICurrentTenantService _tenantService;
    private readonly VencimientoGeneratorEngine _engine;

    public VencimientoService(IAppDbContext context, ICurrentTenantService tenantService, VencimientoGeneratorEngine engine)
    {
        _context = context;
        _tenantService = tenantService;
        _engine = engine;
    }

    public async Task<List<VencimientoDto>> GetVencimientosAsync(
        string? periodo = null,
        Guid? clienteId = null,
        EstadoVencimiento? estado = null,
        string? obligacionId = null,
        Guid? responsableId = null,
        CancellationToken cancellationToken = default)
    {
        var query = _context.Vencimientos
            .Include(v => v.Cliente)
            .Include(v => v.ClienteObligacion)
                .ThenInclude(co => co.Obligacion)
            .Include(v => v.Responsable)
            .Include(v => v.Tareas)
            .Include(v => v.Documentos)
            .AsNoTracking();

        if (!string.IsNullOrWhiteSpace(periodo))
            query = query.Where(v => v.PeriodoFiscal == periodo);

        if (clienteId.HasValue)
            query = query.Where(v => v.ClienteId == clienteId.Value);

        if (estado.HasValue)
            query = query.Where(v => v.Estado == estado.Value);

        if (!string.IsNullOrWhiteSpace(obligacionId))
            query = query.Where(v => v.ClienteObligacion.ObligacionId == obligacionId);

        if (responsableId.HasValue)
            query = query.Where(v => v.ResponsableId == responsableId.Value);

        var list = await query
            .OrderBy(v => v.FechaVencimientoEfectiva)
            .ThenBy(v => v.Cliente.RazonSocial)
            .ToListAsync(cancellationToken);

        return list.Select(MapToDto).ToList();
    }

    public async Task<VencimientoDto?> GetVencimientoByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var v = await _context.Vencimientos
            .Include(x => x.Cliente)
            .Include(x => x.ClienteObligacion)
                .ThenInclude(co => co.Obligacion)
            .Include(x => x.Responsable)
            .Include(x => x.Tareas)
            .Include(x => x.Documentos)
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

        return v != null ? MapToDto(v) : null;
    }

    public async Task<VencimientoDto?> AplicarProrrogaAsync(Guid id, ProrrogaVencimientoDto dto, CancellationToken cancellationToken = default)
    {
        var v = await _context.Vencimientos
            .Include(x => x.Cliente)
            .Include(x => x.ClienteObligacion).ThenInclude(co => co.Obligacion)
            .Include(x => x.Responsable)
            .Include(x => x.Tareas)
            .Include(x => x.Documentos)
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

        if (v == null) return null;

        v.FechaVencimientoEfectiva = dto.NuevaFechaEfectiva;
        v.TieneProrroga = true;
        v.MotivoProrroga = dto.Motivo.Trim();
        v.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);
        return MapToDto(v);
    }

    public async Task<VencimientoDto?> ActualizarEstadoAsync(Guid id, UpdateEstadoVencimientoDto dto, CancellationToken cancellationToken = default)
    {
        var v = await _context.Vencimientos
            .Include(x => x.Cliente)
            .Include(x => x.ClienteObligacion).ThenInclude(co => co.Obligacion)
            .Include(x => x.Responsable)
            .Include(x => x.Tareas)
            .Include(x => x.Documentos)
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

        if (v == null) return null;

        v.Estado = dto.NuevoEstado;
        if (dto.MontoAPagar.HasValue)
            v.MontoAPagar = dto.MontoAPagar.Value;

        if (dto.NuevoEstado == EstadoVencimiento.Presentado && !v.FechaPresentacion.HasValue)
            v.FechaPresentacion = dto.FechaAccion ?? DateTime.UtcNow;

        if (dto.NuevoEstado == EstadoVencimiento.Pagado && !v.FechaPago.HasValue)
            v.FechaPago = dto.FechaAccion ?? DateTime.UtcNow;

        v.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync(cancellationToken);
        return MapToDto(v);
    }

    public async Task<int> GenerarVencimientosPeriodoAsync(GenerarPeriodoDto dto, CancellationToken cancellationToken = default)
    {
        var estudioId = _tenantService.CurrentEstudioId ?? throw new UnauthorizedAccessException("Tenant no identificado.");

        // Parsear "YYYY-MM"
        var partes = dto.PeriodoFiscal.Split('-');
        if (partes.Length != 2 || !int.TryParse(partes[0], out int anio) || !int.TryParse(partes[1], out int mes))
        {
            throw new ArgumentException("El formato del período fiscal debe ser YYYY-MM (ej. 2026-09).");
        }

        var clientesQuery = _context.Clientes
            .Include(c => c.Obligaciones.Where(o => o.Activo))
            .Where(c => c.Activo);

        if (dto.ClienteId.HasValue)
            clientesQuery = clientesQuery.Where(c => c.Id == dto.ClienteId.Value);

        var clientes = await clientesQuery.ToListAsync(cancellationToken);
        int generados = 0;

        foreach (var cliente in clientes)
        {
            foreach (var obligacion in cliente.Obligaciones)
            {
                // Verificar si ya existe el vencimiento para ese período
                bool existe = await _context.Vencimientos.AnyAsync(v => 
                    v.ClienteId == cliente.Id &&
                    v.ClienteObligacionId == obligacion.Id &&
                    v.PeriodoFiscal == dto.PeriodoFiscal, cancellationToken);

                if (existe) continue;

                var fechaCalculada = _engine.CalcularFechaVencimiento(cliente, obligacion, anio, mes);

                var vencimiento = new Vencimiento
                {
                    EstudioId = estudioId,
                    ClienteId = cliente.Id,
                    ClienteObligacionId = obligacion.Id,
                    PeriodoFiscal = dto.PeriodoFiscal,
                    FechaVencimientoOriginal = fechaCalculada,
                    FechaVencimientoEfectiva = fechaCalculada,
                    Estado = EstadoVencimiento.Pendiente,
                    ResponsableId = cliente.ResponsableAsignadoId,
                    CreatedAt = DateTime.UtcNow
                };

                _context.Vencimientos.Add(vencimiento);

                // Generar tareas asociadas al checklist por defecto
                var tareas = _engine.GenerarTareasPorDefecto(vencimiento, cliente.ResponsableAsignadoId);
                foreach (var t in tareas)
                {
                    _context.Tareas.Add(t);
                }

                generados++;
            }
        }

        await _context.SaveChangesAsync(cancellationToken);
        return generados;
    }

    private static VencimientoDto MapToDto(Vencimiento v)
    {
        return new VencimientoDto(
            v.Id,
            v.ClienteId,
            v.Cliente.RazonSocial,
            v.Cliente.Cuit,
            v.ClienteObligacionId,
            v.ClienteObligacion.ObligacionId,
            v.ClienteObligacion.Obligacion?.Nombre ?? v.ClienteObligacion.ObligacionId,
            v.ClienteObligacion.Jurisdiccion,
            v.PeriodoFiscal,
            v.FechaVencimientoOriginal,
            v.FechaVencimientoEfectiva,
            v.TieneProrroga,
            v.MotivoProrroga,
            v.Estado,
            v.MontoAPagar,
            v.FechaPresentacion,
            v.FechaPago,
            v.ResponsableId,
            v.Responsable != null ? $"{v.Responsable.Nombre} {v.Responsable.Apellido}" : null,
            v.Tareas.Count,
            v.Tareas.Count(t => t.Estado == EstadoTarea.Completada),
            v.Documentos.Count,
            v.Notas
        );
    }
}

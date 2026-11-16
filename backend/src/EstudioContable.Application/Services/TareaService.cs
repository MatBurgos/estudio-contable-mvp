using Microsoft.EntityFrameworkCore;
using EstudioContable.Application.Common.Interfaces;
using EstudioContable.Application.DTOs.Tareas;
using EstudioContable.Domain.Entities;
using EstudioContable.Domain.Enums;

namespace EstudioContable.Application.Services;

public class TareaService : ITareaService
{
    private readonly IAppDbContext _context;
    private readonly ICurrentTenantService _tenantService;

    public TareaService(IAppDbContext context, ICurrentTenantService tenantService)
    {
        _context = context;
        _tenantService = tenantService;
    }

    public async Task<List<TareaDto>> GetTareasAsync(
        Guid? clienteId = null,
        Guid? vencimientoId = null,
        Guid? asignadoAId = null,
        EstadoTarea? estado = null,
        CancellationToken cancellationToken = default)
    {
        var query = _context.Tareas
            .Include(t => t.Cliente)
            .Include(t => t.AsignadoA)
            .Include(t => t.Vencimiento)
                .ThenInclude(v => v!.ClienteObligacion)
                    .ThenInclude(co => co.Obligacion)
            .AsNoTracking();

        if (clienteId.HasValue)
            query = query.Where(t => t.ClienteId == clienteId.Value);

        if (vencimientoId.HasValue)
            query = query.Where(t => t.VencimientoId == vencimientoId.Value);

        if (asignadoAId.HasValue)
            query = query.Where(t => t.AsignadoAUsuarioId == asignadoAId.Value);

        if (estado.HasValue)
            query = query.Where(t => t.Estado == estado.Value);

        var list = await query
            .OrderBy(t => t.Estado)
            .ThenBy(t => t.FechaLimite)
            .ThenBy(t => t.Orden)
            .ToListAsync(cancellationToken);

        return list.Select(MapToDto).ToList();
    }

    public async Task<TareaDto?> GetTareaByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var t = await _context.Tareas
            .Include(t => t.Cliente)
            .Include(t => t.AsignadoA)
            .Include(t => t.Vencimiento)
                .ThenInclude(v => v!.ClienteObligacion)
                    .ThenInclude(co => co.Obligacion)
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

        return t != null ? MapToDto(t) : null;
    }

    public async Task<TareaDto> CreateTareaAsync(CreateTareaDto dto, CancellationToken cancellationToken = default)
    {
        var estudioId = _tenantService.CurrentEstudioId ?? throw new UnauthorizedAccessException();

        var tarea = new Tarea
        {
            EstudioId = estudioId,
            ClienteId = dto.ClienteId,
            VencimientoId = dto.VencimientoId,
            Titulo = dto.Titulo.Trim(),
            Descripcion = dto.Descripcion?.Trim(),
            AsignadoAUsuarioId = dto.AsignadoAUsuarioId,
            Estado = EstadoTarea.Pendiente,
            Prioridad = dto.Prioridad,
            FechaLimite = dto.FechaLimite,
            CreatedAt = DateTime.UtcNow
        };

        _context.Tareas.Add(tarea);
        await _context.SaveChangesAsync(cancellationToken);

        return (await GetTareaByIdAsync(tarea.Id, cancellationToken))!;
    }

    public async Task<TareaDto?> UpdateTareaAsync(Guid id, UpdateTareaDto dto, CancellationToken cancellationToken = default)
    {
        var tarea = await _context.Tareas.FirstOrDefaultAsync(t => t.Id == id, cancellationToken);
        if (tarea == null) return null;

        tarea.Titulo = dto.Titulo.Trim();
        tarea.Descripcion = dto.Descripcion?.Trim();
        tarea.AsignadoAUsuarioId = dto.AsignadoAUsuarioId;
        tarea.Estado = dto.Estado;
        tarea.Prioridad = dto.Prioridad;
        tarea.FechaLimite = dto.FechaLimite;
        tarea.Orden = dto.Orden;
        tarea.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);
        return await GetTareaByIdAsync(id, cancellationToken);
    }

    public async Task<TareaDto?> UpdateEstadoAsync(Guid id, UpdateEstadoTareaDto dto, CancellationToken cancellationToken = default)
    {
        var tarea = await _context.Tareas.FirstOrDefaultAsync(t => t.Id == id, cancellationToken);
        if (tarea == null) return null;

        tarea.Estado = dto.NuevoEstado;
        tarea.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);
        return await GetTareaByIdAsync(id, cancellationToken);
    }

    public async Task<bool> DeleteTareaAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var tarea = await _context.Tareas.FirstOrDefaultAsync(t => t.Id == id, cancellationToken);
        if (tarea == null) return false;

        _context.Tareas.Remove(tarea);
        await _context.SaveChangesAsync(cancellationToken);
        return true;
    }

    private static TareaDto MapToDto(Tarea t)
    {
        string? vencInfo = null;
        if (t.Vencimiento != null)
        {
            var imp = t.Vencimiento.ClienteObligacion?.Obligacion?.Nombre ?? t.Vencimiento.ClienteObligacion?.ObligacionId ?? "Impuesto";
            vencInfo = $"{imp} ({t.Vencimiento.PeriodoFiscal})";
        }

        return new TareaDto(
            t.Id,
            t.ClienteId,
            t.Cliente.RazonSocial,
            t.VencimientoId,
            vencInfo,
            t.Titulo,
            t.Descripcion,
            t.AsignadoAUsuarioId,
            t.AsignadoA != null ? $"{t.AsignadoA.Nombre} {t.AsignadoA.Apellido}" : null,
            t.Estado,
            t.Prioridad,
            t.FechaLimite,
            t.Orden,
            t.CreatedAt
        );
    }
}

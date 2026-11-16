using Microsoft.EntityFrameworkCore;
using EstudioContable.Application.Common.Interfaces;
using EstudioContable.Application.DTOs.Clientes;
using EstudioContable.Domain.Entities;

namespace EstudioContable.Application.Services;

public class ClienteService : IClienteService
{
    private readonly IAppDbContext _context;
    private readonly ICurrentTenantService _tenantService;

    public ClienteService(IAppDbContext _context, ICurrentTenantService tenantService)
    {
        this._context = _context;
        _tenantService = tenantService;
    }

    public async Task<List<ClienteListDto>> GetClientesAsync(string? search = null, bool soloActivos = true, CancellationToken cancellationToken = default)
    {
        var query = _context.Clientes
            .Include(c => c.ResponsableAsignado)
            .Include(c => c.Obligaciones)
            .AsNoTracking();

        if (soloActivos)
            query = query.Where(c => c.Activo);

        if (!string.IsNullOrWhiteSpace(search))
        {
            var s = search.Trim().ToLower();
            query = query.Where(c => c.RazonSocial.ToLower().Contains(s) || c.Cuit.Contains(s) || (c.NombreFantasia != null && c.NombreFantasia.ToLower().Contains(s)));
        }

        return await query.Select(c => new ClienteListDto(
            c.Id,
            c.Cuit,
            c.RazonSocial,
            c.NombreFantasia,
            c.TipoPersona,
            c.RegimenTributario,
            c.EmailContacto,
            c.TelefonoContacto,
            c.ResponsableAsignado != null ? $"{c.ResponsableAsignado.Nombre} {c.ResponsableAsignado.Apellido}" : null,
            c.Obligaciones.Count(o => o.Activo),
            c.Activo
        )).ToListAsync(cancellationToken);
    }

    public async Task<ClienteDetailDto?> GetClienteByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var c = await _context.Clientes
            .Include(c => c.ResponsableAsignado)
            .Include(c => c.Obligaciones)
                .ThenInclude(o => o.Obligacion)
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

        if (c == null) return null;

        return new ClienteDetailDto(
            c.Id,
            c.Cuit,
            c.RazonSocial,
            c.NombreFantasia,
            c.TipoPersona,
            c.RegimenTributario,
            c.CierreEjercicioMes,
            c.EmailContacto,
            c.TelefonoContacto,
            c.ResponsableAsignadoId,
            c.ResponsableAsignado != null ? $"{c.ResponsableAsignado.Nombre} {c.ResponsableAsignado.Apellido}" : null,
            c.Notas,
            c.Activo,
            c.Obligaciones.Select(o => new ClienteObligacionDto(
                o.Id,
                o.ObligacionId,
                o.Obligacion.Nombre,
                o.Obligacion.Organismo,
                o.Obligacion.Periodicidad,
                o.Jurisdiccion,
                o.DiaVencimientoPersonalizado,
                o.Activo
            )).ToList()
        );
    }

    public async Task<ClienteDetailDto> CreateClienteAsync(CreateClienteDto dto, CancellationToken cancellationToken = default)
    {
        var estudioId = _tenantService.CurrentEstudioId ?? throw new UnauthorizedAccessException("No se encontró tenant activo.");

        var cliente = new Cliente
        {
            EstudioId = estudioId,
            Cuit = dto.Cuit.Replace("-", "").Trim(),
            RazonSocial = dto.RazonSocial.Trim(),
            NombreFantasia = dto.NombreFantasia?.Trim(),
            TipoPersona = dto.TipoPersona,
            RegimenTributario = dto.RegimenTributario,
            CierreEjercicioMes = dto.CierreEjercicioMes,
            EmailContacto = dto.EmailContacto?.Trim(),
            TelefonoContacto = dto.TelefonoContacto?.Trim(),
            ResponsableAsignadoId = dto.ResponsableAsignadoId,
            Notas = dto.Notas,
            Activo = true
        };

        if (dto.ObligacionesIdsIniciales != null && dto.ObligacionesIdsIniciales.Any())
        {
            foreach (var obligacionId in dto.ObligacionesIdsIniciales)
            {
                cliente.Obligaciones.Add(new ClienteObligacion
                {
                    EstudioId = estudioId,
                    ObligacionId = obligacionId,
                    Activo = true
                });
            }
        }

        _context.Clientes.Add(cliente);
        await _context.SaveChangesAsync(cancellationToken);

        return (await GetClienteByIdAsync(cliente.Id, cancellationToken))!;
    }

    public async Task<ClienteDetailDto?> UpdateClienteAsync(Guid id, UpdateClienteDto dto, CancellationToken cancellationToken = default)
    {
        var cliente = await _context.Clientes.FirstOrDefaultAsync(c => c.Id == id, cancellationToken);
        if (cliente == null) return null;

        cliente.RazonSocial = dto.RazonSocial.Trim();
        cliente.NombreFantasia = dto.NombreFantasia?.Trim();
        cliente.TipoPersona = dto.TipoPersona;
        cliente.RegimenTributario = dto.RegimenTributario;
        cliente.CierreEjercicioMes = dto.CierreEjercicioMes;
        cliente.EmailContacto = dto.EmailContacto?.Trim();
        cliente.TelefonoContacto = dto.TelefonoContacto?.Trim();
        cliente.ResponsableAsignadoId = dto.ResponsableAsignadoId;
        cliente.Notas = dto.Notas;
        cliente.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);
        return await GetClienteByIdAsync(id, cancellationToken);
    }

    public async Task<bool> DeleteClienteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var cliente = await _context.Clientes.FirstOrDefaultAsync(c => c.Id == id, cancellationToken);
        if (cliente == null) return false;

        // Baja lógica
        cliente.Activo = false;
        cliente.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync(cancellationToken);
        return true;
    }

    public async Task<bool> AsignarObligacionAsync(Guid clienteId, AsignarObligacionDto dto, CancellationToken cancellationToken = default)
    {
        var cliente = await _context.Clientes
            .Include(c => c.Obligaciones)
            .FirstOrDefaultAsync(c => c.Id == clienteId, cancellationToken);

        if (cliente == null) return false;

        var existente = cliente.Obligaciones.FirstOrDefault(o => o.ObligacionId.Equals(dto.ObligacionId, StringComparison.OrdinalIgnoreCase));
        if (existente != null)
        {
            existente.Activo = true;
            existente.Jurisdiccion = dto.Jurisdiccion;
            existente.DiaVencimientoPersonalizado = dto.DiaVencimientoPersonalizado;
        }
        else
        {
            var estudioId = _tenantService.CurrentEstudioId ?? throw new UnauthorizedAccessException();
            cliente.Obligaciones.Add(new ClienteObligacion
            {
                EstudioId = estudioId,
                ClienteId = clienteId,
                ObligacionId = dto.ObligacionId,
                Jurisdiccion = dto.Jurisdiccion,
                DiaVencimientoPersonalizado = dto.DiaVencimientoPersonalizado,
                Activo = true
            });
        }

        await _context.SaveChangesAsync(cancellationToken);
        return true;
    }

    public async Task<bool> DesasignarObligacionAsync(Guid clienteId, Guid clienteObligacionId, CancellationToken cancellationToken = default)
    {
        var obligacion = await _context.ClienteObligaciones
            .FirstOrDefaultAsync(o => o.Id == clienteObligacionId && o.ClienteId == clienteId, cancellationToken);

        if (obligacion == null) return false;

        obligacion.Activo = false;
        await _context.SaveChangesAsync(cancellationToken);
        return true;
    }
}

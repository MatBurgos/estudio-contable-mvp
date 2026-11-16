using Microsoft.EntityFrameworkCore;
using EstudioContable.Application.Common.Interfaces;
using EstudioContable.Application.DTOs.PortalCliente;
using EstudioContable.Domain.Enums;

namespace EstudioContable.Application.Services;

public class PortalClienteService : IPortalClienteService
{
    private readonly IAppDbContext _context;
    private readonly ICurrentTenantService _tenantService;

    public PortalClienteService(IAppDbContext context, ICurrentTenantService tenantService)
    {
        _context = context;
        _tenantService = tenantService;
    }

    public async Task<PortalResumenDto> GetMiResumenAsync(CancellationToken cancellationToken = default)
    {
        var clienteId = _tenantService.CurrentClienteId 
            ?? throw new UnauthorizedAccessException("El usuario autenticado no está vinculado a una ficha de cliente.");

        var cliente = await _context.Clientes
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.Id == clienteId, cancellationToken)
            ?? throw new KeyNotFoundException("Cliente no encontrado.");

        var mesActualStr = DateTime.UtcNow.ToString("yyyy-MM");

        var vencimientosMes = await _context.Vencimientos
            .Include(v => v.ClienteObligacion).ThenInclude(co => co.Obligacion)
            .Include(v => v.Documentos.Where(d => d.VisibleParaCliente))
            .Where(v => v.ClienteId == clienteId && v.PeriodoFiscal == mesActualStr)
            .OrderBy(v => v.FechaVencimientoEfectiva)
            .AsNoTracking()
            .ToListAsync(cancellationToken);

        int enProceso = vencimientosMes.Count(v => v.Estado == EstadoVencimiento.Pendiente || v.Estado == EstadoVencimiento.EnProceso);
        int presentados = vencimientosMes.Count(v => v.Estado == EstadoVencimiento.Presentado);
        int pagados = vencimientosMes.Count(v => v.Estado == EstadoVencimiento.Pagado);
        decimal montoPendiente = vencimientosMes
            .Where(v => v.Estado == EstadoVencimiento.Presentado && v.MontoAPagar.HasValue)
            .Sum(v => v.MontoAPagar!.Value);

        var vencimientosDto = vencimientosMes.Select(v => new PortalVencimientoDto(
            v.Id,
            v.ClienteObligacion.Obligacion?.Nombre ?? v.ClienteObligacion.ObligacionId,
            v.PeriodoFiscal,
            v.FechaVencimientoEfectiva,
            v.Estado,
            v.MontoAPagar,
            v.FechaPresentacion,
            v.FechaPago,
            v.Documentos.Select(d => new PortalDocumentoDto(
                d.Id,
                d.NombreOriginal,
                d.NombreOriginal,
                d.Categoria.ToString(),
                d.TamanoBytes,
                d.CreatedAt
            )).ToList()
        )).ToList();

        return new PortalResumenDto(
            cliente.RazonSocial,
            cliente.Cuit,
            cliente.RegimenTributario.ToString(),
            mesActualStr,
            enProceso,
            presentados,
            pagados,
            montoPendiente,
            vencimientosDto
        );
    }

    public async Task<List<PortalVencimientoDto>> GetMisVencimientosAsync(string? periodo = null, CancellationToken cancellationToken = default)
    {
        var clienteId = _tenantService.CurrentClienteId 
            ?? throw new UnauthorizedAccessException("El usuario autenticado no está vinculado a una ficha de cliente.");

        var query = _context.Vencimientos
            .Include(v => v.ClienteObligacion).ThenInclude(co => co.Obligacion)
            .Include(v => v.Documentos.Where(d => d.VisibleParaCliente))
            .Where(v => v.ClienteId == clienteId)
            .AsNoTracking();

        if (!string.IsNullOrWhiteSpace(periodo))
            query = query.Where(v => v.PeriodoFiscal == periodo);

        var list = await query
            .OrderByDescending(v => v.PeriodoFiscal)
            .ThenBy(v => v.FechaVencimientoEfectiva)
            .ToListAsync(cancellationToken);

        return list.Select(v => new PortalVencimientoDto(
            v.Id,
            v.ClienteObligacion.Obligacion?.Nombre ?? v.ClienteObligacion.ObligacionId,
            v.PeriodoFiscal,
            v.FechaVencimientoEfectiva,
            v.Estado,
            v.MontoAPagar,
            v.FechaPresentacion,
            v.FechaPago,
            v.Documentos.Select(d => new PortalDocumentoDto(
                d.Id,
                d.NombreOriginal,
                d.NombreOriginal,
                d.Categoria.ToString(),
                d.TamanoBytes,
                d.CreatedAt
            )).ToList()
        )).ToList();
    }

    public async Task<List<PortalDocumentoDto>> GetMisDocumentosAsync(CancellationToken cancellationToken = default)
    {
        var clienteId = _tenantService.CurrentClienteId 
            ?? throw new UnauthorizedAccessException("El usuario autenticado no está vinculado a una ficha de cliente.");

        var docs = await _context.Documentos
            .Where(d => d.ClienteId == clienteId && d.VisibleParaCliente)
            .OrderByDescending(d => d.CreatedAt)
            .AsNoTracking()
            .ToListAsync(cancellationToken);

        return docs.Select(d => new PortalDocumentoDto(
            d.Id,
            d.NombreOriginal,
            d.NombreOriginal,
            d.Categoria.ToString(),
            d.TamanoBytes,
            d.CreatedAt
        )).ToList();
    }
}

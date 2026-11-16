using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using EstudioContable.Application.Common.Interfaces;
using EstudioContable.Application.DTOs.Dashboard;
using EstudioContable.Application.Services;
using EstudioContable.Domain.Enums;

namespace EstudioContable.WebApi.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "AdminEstudio,Colaborador")]
public class DashboardController : ControllerBase
{
    private readonly IAppDbContext _context;
    private readonly ICurrentTenantService _tenantService;
    private readonly IVencimientoService _vencimientoService;
    private readonly ITareaService _tareaService;

    public DashboardController(
        IAppDbContext context,
        ICurrentTenantService tenantService,
        IVencimientoService vencimientoService,
        ITareaService tareaService)
    {
        _context = context;
        _tenantService = tenantService;
        _vencimientoService = vencimientoService;
        _tareaService = tareaService;
    }

    [HttpGet("resumen")]
    public async Task<IActionResult> GetResumen(CancellationToken cancellationToken)
    {
        var hoy = DateOnly.FromDateTime(DateTime.UtcNow);
        var en15Dias = hoy.AddDays(15);
        var mesActualStr = DateTime.UtcNow.ToString("yyyy-MM");

        var totalClientes = await _context.Clientes.CountAsync(c => c.Activo, cancellationToken);

        var vencimientosMes = await _vencimientoService.GetVencimientosAsync(periodo: mesActualStr, cancellationToken: cancellationToken);

        int presentados = vencimientosMes.Count(v => v.Estado == EstadoVencimiento.Presentado || v.Estado == EstadoVencimiento.Pagado);
        int pendientes = vencimientosMes.Count(v => v.Estado == EstadoVencimiento.Pendiente || v.Estado == EstadoVencimiento.EnProceso);
        int vencidos = vencimientosMes.Count(v => v.FechaVencimientoEfectiva < hoy && v.Estado != EstadoVencimiento.Presentado && v.Estado != EstadoVencimiento.Pagado);

        var proximos = vencimientosMes
            .Where(v => v.FechaVencimientoEfectiva >= hoy && v.FechaVencimientoEfectiva <= en15Dias && v.Estado != EstadoVencimiento.Presentado && v.Estado != EstadoVencimiento.Pagado)
            .OrderBy(v => v.FechaVencimientoEfectiva)
            .Take(10)
            .ToList();

        var urgentes = vencimientosMes
            .Where(v => (v.FechaVencimientoEfectiva <= hoy.AddDays(2) || v.FechaVencimientoEfectiva < hoy) && v.Estado != EstadoVencimiento.Presentado && v.Estado != EstadoVencimiento.Pagado)
            .OrderBy(v => v.FechaVencimientoEfectiva)
            .ToList();

        var userId = _tenantService.CurrentUserId;
        var misTareas = await _tareaService.GetTareasAsync(
            asignadoAId: userId,
            estado: EstadoTarea.Pendiente,
            cancellationToken: cancellationToken);

        int totalTareasPendientes = await _context.Tareas.CountAsync(t => t.Estado == EstadoTarea.Pendiente || t.Estado == EstadoTarea.EnProceso, cancellationToken);

        var resumen = new DashboardResumenDto(
            totalClientes,
            vencimientosMes.Count,
            presentados,
            pendientes,
            vencidos,
            totalTareasPendientes,
            proximos,
            urgentes,
            misTareas.Take(8).ToList()
        );

        return Ok(resumen);
    }
}

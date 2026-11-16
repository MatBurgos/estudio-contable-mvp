using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using EstudioContable.Application.DTOs.Vencimientos;
using EstudioContable.Application.Services;
using EstudioContable.Domain.Enums;

namespace EstudioContable.WebApi.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "AdminEstudio,Colaborador")]
public class VencimientosController : ControllerBase
{
    private readonly IVencimientoService _vencimientoService;

    public VencimientosController(IVencimientoService vencimientoService)
    {
        _vencimientoService = vencimientoService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll(
        [FromQuery] string? periodo,
        [FromQuery] Guid? clienteId,
        [FromQuery] EstadoVencimiento? estado,
        [FromQuery] string? obligacionId,
        [FromQuery] Guid? responsableId,
        CancellationToken cancellationToken)
    {
        var list = await _vencimientoService.GetVencimientosAsync(
            periodo, clienteId, estado, obligacionId, responsableId, cancellationToken);

        return Ok(list);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
    {
        var v = await _vencimientoService.GetVencimientoByIdAsync(id, cancellationToken);
        if (v == null) return NotFound(new { message = "Vencimiento no encontrado." });

        return Ok(v);
    }

    [HttpPatch("{id:guid}/prorroga")]
    public async Task<IActionResult> AplicarProrroga(Guid id, [FromBody] ProrrogaVencimientoDto dto, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(dto.Motivo))
            return BadRequest(new { message = "Debe especificar un motivo para la prórroga." });

        var actualizado = await _vencimientoService.AplicarProrrogaAsync(id, dto, cancellationToken);
        if (actualizado == null) return NotFound(new { message = "Vencimiento no encontrado." });

        return Ok(actualizado);
    }

    [HttpPatch("{id:guid}/estado")]
    public async Task<IActionResult> ActualizarEstado(Guid id, [FromBody] UpdateEstadoVencimientoDto dto, CancellationToken cancellationToken)
    {
        var actualizado = await _vencimientoService.ActualizarEstadoAsync(id, dto, cancellationToken);
        if (actualizado == null) return NotFound(new { message = "Vencimiento no encontrado." });

        return Ok(actualizado);
    }

    [HttpPost("generar-periodo")]
    public async Task<IActionResult> GenerarPeriodo([FromBody] GenerarPeriodoDto dto, CancellationToken cancellationToken)
    {
        try
        {
            int totalGenerados = await _vencimientoService.GenerarVencimientosPeriodoAsync(dto, cancellationToken);
            return Ok(new 
            { 
                message = $"Se generaron {totalGenerados} vencimientos y sus checklists de tareas para el período {dto.PeriodoFiscal}.",
                total = totalGenerados
            });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }
}

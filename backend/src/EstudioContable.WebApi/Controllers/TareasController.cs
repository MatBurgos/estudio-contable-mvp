using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using EstudioContable.Application.DTOs.Tareas;
using EstudioContable.Application.Services;
using EstudioContable.Domain.Enums;

namespace EstudioContable.WebApi.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "AdminEstudio,Colaborador")]
public class TareasController : ControllerBase
{
    private readonly ITareaService _tareaService;

    public TareasController(ITareaService tareaService)
    {
        _tareaService = tareaService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll(
        [FromQuery] Guid? clienteId,
        [FromQuery] Guid? vencimientoId,
        [FromQuery] Guid? asignadoAId,
        [FromQuery] EstadoTarea? estado,
        CancellationToken cancellationToken)
    {
        var tareas = await _tareaService.GetTareasAsync(clienteId, vencimientoId, asignadoAId, estado, cancellationToken);
        return Ok(tareas);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
    {
        var tarea = await _tareaService.GetTareaByIdAsync(id, cancellationToken);
        if (tarea == null) return NotFound(new { message = "Tarea no encontrada." });

        return Ok(tarea);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateTareaDto dto, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(dto.Titulo))
            return BadRequest(new { message = "El título de la tarea es obligatorio." });

        var creada = await _tareaService.CreateTareaAsync(dto, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = creada.Id }, creada);
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateTareaDto dto, CancellationToken cancellationToken)
    {
        var actualizada = await _tareaService.UpdateTareaAsync(id, dto, cancellationToken);
        if (actualizada == null) return NotFound(new { message = "Tarea no encontrada." });

        return Ok(actualizada);
    }

    [HttpPatch("{id:guid}/estado")]
    public async Task<IActionResult> UpdateEstado(Guid id, [FromBody] UpdateEstadoTareaDto dto, CancellationToken cancellationToken)
    {
        var actualizada = await _tareaService.UpdateEstadoAsync(id, dto, cancellationToken);
        if (actualizada == null) return NotFound(new { message = "Tarea no encontrada." });

        return Ok(actualizada);
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        var eliminada = await _tareaService.DeleteTareaAsync(id, cancellationToken);
        if (!eliminada) return NotFound(new { message = "Tarea no encontrada." });

        return NoContent();
    }
}

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using EstudioContable.Application.DTOs.Clientes;
using EstudioContable.Application.Services;

namespace EstudioContable.WebApi.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "AdminEstudio,Colaborador")]
public class ClientesController : ControllerBase
{
    private readonly IClienteService _clienteService;

    public ClientesController(IClienteService clienteService)
    {
        _clienteService = clienteService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] string? search, [FromQuery] bool soloActivos = true, CancellationToken cancellationToken = default)
    {
        var clientes = await _clienteService.GetClientesAsync(search, soloActivos, cancellationToken);
        return Ok(clientes);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
    {
        var cliente = await _clienteService.GetClienteByIdAsync(id, cancellationToken);
        if (cliente == null) return NotFound(new { message = "Cliente no encontrado." });

        return Ok(cliente);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateClienteDto dto, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(dto.Cuit) || dto.Cuit.Replace("-", "").Trim().Length != 11)
        {
            return BadRequest(new { message = "El CUIT debe contener 11 dígitos numéricos." });
        }

        var creado = await _clienteService.CreateClienteAsync(dto, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = creado.Id }, creado);
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateClienteDto dto, CancellationToken cancellationToken)
    {
        var actualizado = await _clienteService.UpdateClienteAsync(id, dto, cancellationToken);
        if (actualizado == null) return NotFound(new { message = "Cliente no encontrado." });

        return Ok(actualizado);
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        var eliminado = await _clienteService.DeleteClienteAsync(id, cancellationToken);
        if (!eliminado) return NotFound(new { message = "Cliente no encontrado." });

        return NoContent();
    }

    [HttpPost("{id:guid}/obligaciones")]
    public async Task<IActionResult> AsignarObligacion(Guid id, [FromBody] AsignarObligacionDto dto, CancellationToken cancellationToken)
    {
        var exito = await _clienteService.AsignarObligacionAsync(id, dto, cancellationToken);
        if (!exito) return BadRequest(new { message = "No se pudo asignar la obligación al cliente." });

        return Ok(new { message = "Obligación asignada correctamente." });
    }

    [HttpDelete("{id:guid}/obligaciones/{clienteObligacionId:guid}")]
    public async Task<IActionResult> DesasignarObligacion(Guid id, Guid clienteObligacionId, CancellationToken cancellationToken)
    {
        var exito = await _clienteService.DesasignarObligacionAsync(id, clienteObligacionId, cancellationToken);
        if (!exito) return NotFound(new { message = "Obligación no encontrada para este cliente." });

        return NoContent();
    }
}

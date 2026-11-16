using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using EstudioContable.Application.DTOs.Documentos;
using EstudioContable.Application.Services;
using EstudioContable.Domain.Enums;

namespace EstudioContable.WebApi.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "AdminEstudio,Colaborador")]
public class DocumentosController : ControllerBase
{
    private readonly IDocumentoService _documentoService;

    public DocumentosController(IDocumentoService documentoService)
    {
        _documentoService = documentoService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll(
        [FromQuery] Guid? clienteId,
        [FromQuery] Guid? vencimientoId,
        [FromQuery] CategoriaDocumento? categoria,
        CancellationToken cancellationToken)
    {
        var list = await _documentoService.GetDocumentosAsync(clienteId, vencimientoId, categoria, cancellationToken);
        return Ok(list);
    }

    [HttpPost("upload")]
    [Consumes("multipart/form-data")]
    public async Task<IActionResult> Upload(
        [FromForm] IFormFile file,
        [FromForm] Guid clienteId,
        [FromForm] Guid? vencimientoId,
        [FromForm] CategoriaDocumento categoria,
        [FromForm] bool visibleParaCliente = true,
        CancellationToken cancellationToken = default)
    {
        if (file == null || file.Length == 0)
            return BadRequest(new { message = "Debe seleccionar un archivo válido." });

        // Límite de 20MB para el MVP
        if (file.Length > 20 * 1024 * 1024)
            return BadRequest(new { message = "El archivo supera el tamaño máximo permitido (20MB)." });

        var metadata = new UploadDocumentoMetadataDto(clienteId, vencimientoId, categoria, visibleParaCliente);

        using var stream = file.OpenReadStream();
        var creado = await _documentoService.SubirDocumentoAsync(
            stream, file.FileName, file.ContentType, file.Length, metadata, cancellationToken);

        return Ok(creado);
    }

    [HttpGet("{id:guid}/download")]
    public async Task<IActionResult> Download(Guid id, CancellationToken cancellationToken)
    {
        var resultado = await _documentoService.DescargarDocumentoAsync(id, cancellationToken);
        if (resultado == null) return NotFound(new { message = "Documento no encontrado." });

        return File(resultado.Value.Stream, resultado.Value.ContentType, resultado.Value.FileName);
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        var eliminado = await _documentoService.DeleteDocumentoAsync(id, cancellationToken);
        if (!eliminado) return NotFound(new { message = "Documento no encontrado." });

        return NoContent();
    }
}

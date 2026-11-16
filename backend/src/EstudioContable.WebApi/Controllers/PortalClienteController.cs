using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using EstudioContable.Application.Services;

namespace EstudioContable.WebApi.Controllers;

[ApiController]
[Route("api/portal-cliente")]
[Authorize(Roles = "ClientePortal")]
public class PortalClienteController : ControllerBase
{
    private readonly IPortalClienteService _portalService;
    private readonly IDocumentoService _documentoService;

    public PortalClienteController(IPortalClienteService portalService, IDocumentoService documentoService)
    {
        _portalService = portalService;
        _documentoService = documentoService;
    }

    [HttpGet("resumen")]
    public async Task<IActionResult> GetMiResumen(CancellationToken cancellationToken)
    {
        var resumen = await _portalService.GetMiResumenAsync(cancellationToken);
        return Ok(resumen);
    }

    [HttpGet("vencimientos")]
    public async Task<IActionResult> GetMisVencimientos([FromQuery] string? periodo, CancellationToken cancellationToken)
    {
        var lista = await _portalService.GetMisVencimientosAsync(periodo, cancellationToken);
        return Ok(lista);
    }

    [HttpGet("documentos")]
    public async Task<IActionResult> GetMisDocumentos(CancellationToken cancellationToken)
    {
        var docs = await _portalService.GetMisDocumentosAsync(cancellationToken);
        return Ok(docs);
    }

    [HttpGet("documentos/{id:guid}/download")]
    public async Task<IActionResult> DescargarDocumento(Guid id, CancellationToken cancellationToken)
    {
        var res = await _documentoService.DescargarDocumentoAsync(id, cancellationToken);
        if (res == null) return NotFound(new { message = "Documento no disponible." });

        return File(res.Value.Stream, res.Value.ContentType, res.Value.FileName);
    }
}

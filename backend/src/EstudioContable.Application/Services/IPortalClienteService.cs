using EstudioContable.Application.DTOs.PortalCliente;

namespace EstudioContable.Application.Services;

public interface IPortalClienteService
{
    Task<PortalResumenDto> GetMiResumenAsync(CancellationToken cancellationToken = default);
    Task<List<PortalVencimientoDto>> GetMisVencimientosAsync(string? periodo = null, CancellationToken cancellationToken = default);
    Task<List<PortalDocumentoDto>> GetMisDocumentosAsync(CancellationToken cancellationToken = default);
}

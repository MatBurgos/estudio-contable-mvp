using EstudioContable.Application.DTOs.Documentos;
using EstudioContable.Domain.Enums;

namespace EstudioContable.Application.Services;

public interface IDocumentoService
{
    Task<List<DocumentoDto>> GetDocumentosAsync(
        Guid? clienteId = null,
        Guid? vencimientoId = null,
        CategoriaDocumento? categoria = null,
        CancellationToken cancellationToken = default);

    Task<DocumentoDto?> GetDocumentoByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<DocumentoDto> SubirDocumentoAsync(
        Stream stream,
        string fileName,
        string contentType,
        long length,
        UploadDocumentoMetadataDto metadata,
        CancellationToken cancellationToken = default);

    Task<(Stream Stream, string ContentType, string FileName)?> DescargarDocumentoAsync(Guid id, CancellationToken cancellationToken = default);
    Task<bool> DeleteDocumentoAsync(Guid id, CancellationToken cancellationToken = default);
}

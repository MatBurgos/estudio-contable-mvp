using EstudioContable.Domain.Enums;

namespace EstudioContable.Application.DTOs.Documentos;

public record DocumentoDto(
    Guid Id,
    Guid ClienteId,
    string ClienteRazonSocial,
    Guid? VencimientoId,
    string? VencimientoInfo,
    string NombreOriginal,
    string ContentType,
    long TamanoBytes,
    CategoriaDocumento Categoria,
    bool VisibleParaCliente,
    Guid SubidoPorUsuarioId,
    string SubidoPorNombre,
    DateTime CreatedAt
);

public record UploadDocumentoMetadataDto(
    Guid ClienteId,
    Guid? VencimientoId,
    CategoriaDocumento Categoria,
    bool VisibleParaCliente
);

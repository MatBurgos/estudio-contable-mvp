using Microsoft.EntityFrameworkCore;
using EstudioContable.Application.Common.Interfaces;
using EstudioContable.Application.DTOs.Documentos;
using EstudioContable.Domain.Entities;
using EstudioContable.Domain.Enums;

namespace EstudioContable.Application.Services;

public class DocumentoService : IDocumentoService
{
    private readonly IAppDbContext _context;
    private readonly ICurrentTenantService _tenantService;
    private readonly IFileStorageService _fileStorage;

    public DocumentoService(IAppDbContext context, ICurrentTenantService tenantService, IFileStorageService fileStorage)
    {
        _context = context;
        _tenantService = tenantService;
        _fileStorage = fileStorage;
    }

    public async Task<List<DocumentoDto>> GetDocumentosAsync(
        Guid? clienteId = null,
        Guid? vencimientoId = null,
        CategoriaDocumento? categoria = null,
        CancellationToken cancellationToken = default)
    {
        var query = _context.Documentos
            .Include(d => d.Cliente)
            .Include(d => d.SubidoPor)
            .Include(d => d.Vencimiento)
                .ThenInclude(v => v!.ClienteObligacion)
                    .ThenInclude(co => co.Obligacion)
            .AsNoTracking();

        if (clienteId.HasValue)
            query = query.Where(d => d.ClienteId == clienteId.Value);

        if (vencimientoId.HasValue)
            query = query.Where(d => d.VencimientoId == vencimientoId.Value);

        if (categoria.HasValue)
            query = query.Where(d => d.Categoria == categoria.Value);

        var list = await query
            .OrderByDescending(d => d.CreatedAt)
            .ToListAsync(cancellationToken);

        return list.Select(MapToDto).ToList();
    }

    public async Task<DocumentoDto?> GetDocumentoByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var d = await _context.Documentos
            .Include(x => x.Cliente)
            .Include(x => x.SubidoPor)
            .Include(x => x.Vencimiento)
                .ThenInclude(v => v!.ClienteObligacion)
                    .ThenInclude(co => co.Obligacion)
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

        return d != null ? MapToDto(d) : null;
    }

    public async Task<DocumentoDto> SubirDocumentoAsync(
        Stream stream,
        string fileName,
        string contentType,
        long length,
        UploadDocumentoMetadataDto metadata,
        CancellationToken cancellationToken = default)
    {
        var estudioId = _tenantService.CurrentEstudioId ?? throw new UnauthorizedAccessException();
        var usuarioId = _tenantService.CurrentUserId ?? throw new UnauthorizedAccessException();

        string storagePath = await _fileStorage.UploadFileAsync(stream, fileName, contentType, estudioId, cancellationToken);

        var documento = new Documento
        {
            EstudioId = estudioId,
            ClienteId = metadata.ClienteId,
            VencimientoId = metadata.VencimientoId,
            NombreOriginal = Path.GetFileName(fileName),
            NombreAlmacenado = Path.GetFileName(storagePath),
            RutaAlmacenamiento = storagePath,
            ContentType = string.IsNullOrWhiteSpace(contentType) ? "application/octet-stream" : contentType,
            TamanoBytes = length,
            Categoria = metadata.Categoria,
            VisibleParaCliente = metadata.VisibleParaCliente,
            SubidoPorUsuarioId = usuarioId,
            CreatedAt = DateTime.UtcNow
        };

        _context.Documentos.Add(documento);
        await _context.SaveChangesAsync(cancellationToken);

        return (await GetDocumentoByIdAsync(documento.Id, cancellationToken))!;
    }

    public async Task<(Stream Stream, string ContentType, string FileName)?> DescargarDocumentoAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var doc = await _context.Documentos.FirstOrDefaultAsync(d => d.Id == id, cancellationToken);
        if (doc == null) return null;

        var downloadResult = await _fileStorage.DownloadFileAsync(doc.RutaAlmacenamiento, cancellationToken);
        return (downloadResult.Stream, doc.ContentType, doc.NombreOriginal);
    }

    public async Task<bool> DeleteDocumentoAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var doc = await _context.Documentos.FirstOrDefaultAsync(d => d.Id == id, cancellationToken);
        if (doc == null) return false;

        await _fileStorage.DeleteFileAsync(doc.RutaAlmacenamiento, cancellationToken);
        _context.Documentos.Remove(doc);
        await _context.SaveChangesAsync(cancellationToken);
        return true;
    }

    private static DocumentoDto MapToDto(Documento d)
    {
        string? vencInfo = null;
        if (d.Vencimiento != null)
        {
            var imp = d.Vencimiento.ClienteObligacion?.Obligacion?.Nombre ?? "Impuesto";
            vencInfo = $"{imp} ({d.Vencimiento.PeriodoFiscal})";
        }

        return new DocumentoDto(
            d.Id,
            d.ClienteId,
            d.Cliente.RazonSocial,
            d.VencimientoId,
            vencInfo,
            d.NombreOriginal,
            d.ContentType,
            d.TamanoBytes,
            d.Categoria,
            d.VisibleParaCliente,
            d.SubidoPorUsuarioId,
            $"{d.SubidoPor?.Nombre} {d.SubidoPor?.Apellido}".Trim(),
            d.CreatedAt
        );
    }
}

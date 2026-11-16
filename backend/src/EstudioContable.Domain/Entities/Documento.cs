using EstudioContable.Domain.Common;
using EstudioContable.Domain.Enums;

namespace EstudioContable.Domain.Entities;

/// <summary>
/// Documentos digitales del cliente (comprobantes de pago VEP, acuses de presentación F.2002,
/// facturas de compra/venta, extractos bancarios).
/// </summary>
public class Documento : BaseEntity, ITenantEntity
{
    public Guid EstudioId { get; set; }
    public Estudio Estudio { get; set; } = null!;

    public Guid ClienteId { get; set; }
    public Cliente Cliente { get; set; } = null!;

    public Guid? VencimientoId { get; set; }
    public Vencimiento? Vencimiento { get; set; }

    public string NombreOriginal { get; set; } = string.Empty;
    public string NombreAlmacenado { get; set; } = string.Empty;
    public string RutaAlmacenamiento { get; set; } = string.Empty;
    public string ContentType { get; set; } = "application/pdf";
    public long TamanoBytes { get; set; }

    public CategoriaDocumento Categoria { get; set; } = CategoriaDocumento.Otro;

    /// <summary>
    /// Si es true, el cliente final puede ver y descargar este documento desde su portal.
    /// </summary>
    public bool VisibleParaCliente { get; set; } = true;

    public Guid SubidoPorUsuarioId { get; set; }
    public Usuario SubidoPor { get; set; } = null!;
}

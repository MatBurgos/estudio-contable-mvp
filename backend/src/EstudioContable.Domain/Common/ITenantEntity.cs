namespace EstudioContable.Domain.Common;

/// <summary>
/// Interfaz para entidades que pertenecen a un estudio específico.
/// Esencial para el soporte multi-tenant y los Global Query Filters de EF Core.
/// </summary>
public interface ITenantEntity
{
    Guid EstudioId { get; set; }
}

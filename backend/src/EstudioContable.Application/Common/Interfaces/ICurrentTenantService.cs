using EstudioContable.Domain.Enums;

namespace EstudioContable.Application.Common.Interfaces;

public interface ICurrentTenantService
{
    Guid? CurrentEstudioId { get; }
    Guid? CurrentUserId { get; }
    RolUsuario? CurrentUserRole { get; }
    Guid? CurrentClienteId { get; }
    bool IsAuthenticated { get; }
}

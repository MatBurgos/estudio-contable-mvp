using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using EstudioContable.Application.Common.Interfaces;
using EstudioContable.Domain.Enums;

namespace EstudioContable.Infrastructure.Services;

public class CurrentTenantService : ICurrentTenantService
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CurrentTenantService(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public Guid? CurrentEstudioId
    {
        get
        {
            var claim = _httpContextAccessor.HttpContext?.User?.FindFirst("estudio_id");
            if (claim != null && Guid.TryParse(claim.Value, out var id))
                return id;

            // Header alternativo para testing o integraciones
            var header = _httpContextAccessor.HttpContext?.Request?.Headers["X-Tenant-Id"].FirstOrDefault();
            if (header != null && Guid.TryParse(header, out var tenantId))
                return tenantId;

            return null;
        }
    }

    public Guid? CurrentUserId
    {
        get
        {
            var claim = _httpContextAccessor.HttpContext?.User?.FindFirst(ClaimTypes.NameIdentifier) 
                     ?? _httpContextAccessor.HttpContext?.User?.FindFirst("sub");
            if (claim != null && Guid.TryParse(claim.Value, out var id))
                return id;

            return null;
        }
    }

    public RolUsuario? CurrentUserRole
    {
        get
        {
            var claim = _httpContextAccessor.HttpContext?.User?.FindFirst(ClaimTypes.Role)
                     ?? _httpContextAccessor.HttpContext?.User?.FindFirst("rol");
            if (claim != null && Enum.TryParse<RolUsuario>(claim.Value, out var rol))
                return rol;

            return null;
        }
    }

    public Guid? CurrentClienteId
    {
        get
        {
            var claim = _httpContextAccessor.HttpContext?.User?.FindFirst("cliente_id");
            if (claim != null && Guid.TryParse(claim.Value, out var id))
                return id;

            return null;
        }
    }

    public bool IsAuthenticated => _httpContextAccessor.HttpContext?.User?.Identity?.IsAuthenticated ?? false;
}

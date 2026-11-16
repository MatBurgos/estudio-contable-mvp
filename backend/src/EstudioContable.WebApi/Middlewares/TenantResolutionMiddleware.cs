using EstudioContable.Application.Common.Interfaces;

namespace EstudioContable.WebApi.Middlewares;

public class TenantResolutionMiddleware
{
    private readonly RequestDelegate _next;

    public TenantResolutionMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context, ICurrentTenantService tenantService)
    {
        // Si el usuario está autenticado, adjuntamos el header de respuesta para trazabilidad y debugging
        if (tenantService.CurrentEstudioId.HasValue)
        {
            context.Response.Headers.Append("X-Tenant-Id", tenantService.CurrentEstudioId.Value.ToString());
        }

        await _next(context);
    }
}

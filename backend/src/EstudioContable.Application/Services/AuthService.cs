using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using EstudioContable.Application.Common.Interfaces;
using EstudioContable.Application.DTOs.Auth;
using EstudioContable.Domain.Entities;
using EstudioContable.Domain.Enums;

namespace EstudioContable.Application.Services;

public class AuthService : IAuthService
{
    private readonly IAppDbContext _context;
    private readonly IConfiguration _configuration;
    private readonly ICurrentTenantService _tenantService;

    public AuthService(IAppDbContext context, IConfiguration configuration, ICurrentTenantService tenantService)
    {
        _context = context;
        _configuration = configuration;
        _tenantService = tenantService;
    }

    public async Task<AuthResponseDto?> LoginAsync(LoginRequestDto request, CancellationToken cancellationToken = default)
    {
        // En login no podemos usar el tenant filter porque aún no conocemos el tenant del usuario
        // Buscamos el usuario por su email
        var usuario = await _context.Usuarios
            .Include(u => u.Estudio)
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(u => u.Email.ToLower() == request.Email.Trim().ToLower(), cancellationToken);

        if (usuario == null || !usuario.Activo || !usuario.Estudio.Activo)
            return null;

        // Verificación de password (BCrypt o hash simple para MVP)
        if (!VerificarPassword(request.Password, usuario.PasswordHash))
            return null;

        usuario.UltimoAccesoAt = DateTime.UtcNow;
        await _context.SaveChangesAsync(cancellationToken);

        string token = GenerarJwtToken(usuario);

        return new AuthResponseDto(
            token,
            usuario.Id,
            $"{usuario.Nombre} {usuario.Apellido}".Trim(),
            usuario.Email,
            usuario.Rol,
            usuario.EstudioId,
            usuario.Estudio.Nombre,
            usuario.ClienteId
        );
    }

    public async Task<MeDto?> GetCurrentUserAsync(CancellationToken cancellationToken = default)
    {
        if (!_tenantService.CurrentUserId.HasValue) return null;

        var usuario = await _context.Usuarios
            .Include(u => u.Estudio)
            .FirstOrDefaultAsync(u => u.Id == _tenantService.CurrentUserId.Value, cancellationToken);

        if (usuario == null) return null;

        return new MeDto(
            usuario.Id,
            usuario.Nombre,
            usuario.Apellido,
            usuario.Email,
            usuario.Rol,
            usuario.EstudioId,
            usuario.Estudio.Nombre,
            usuario.ClienteId
        );
    }

    private string GenerarJwtToken(Usuario usuario)
    {
        var secret = _configuration["Jwt:Secret"]
            ?? throw new InvalidOperationException("Jwt:Secret no está configurado.");
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, usuario.Id.ToString()),
            new(JwtRegisteredClaimNames.Email, usuario.Email),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new("estudio_id", usuario.EstudioId.ToString()),
            new(ClaimTypes.Role, usuario.Rol.ToString()),
            new("rol", usuario.Rol.ToString()),
            new("nombre", $"{usuario.Nombre} {usuario.Apellido}".Trim())
        };

        if (usuario.ClienteId.HasValue)
        {
            claims.Add(new Claim("cliente_id", usuario.ClienteId.Value.ToString()));
        }

        var token = new JwtSecurityToken(
            issuer: _configuration["Jwt:Issuer"] ?? "EstudioContableApi",
            audience: _configuration["Jwt:Audience"] ?? "EstudioContableClient",
            claims: claims,
            expires: DateTime.UtcNow.AddDays(7),
            signingCredentials: creds
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    private static bool VerificarPassword(string passwordIngresada, string passwordHashGuardado)
    {
        // Soporte de contraseñas de desarrollo para el seed inicial y hash estándar
        if (passwordHashGuardado == passwordIngresada) return true;
        try
        {
            return BCrypt.Net.BCrypt.Verify(passwordIngresada, passwordHashGuardado);
        }
        catch
        {
            return passwordHashGuardado == passwordIngresada;
        }
    }
}

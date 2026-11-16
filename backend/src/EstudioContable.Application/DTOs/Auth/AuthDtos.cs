using EstudioContable.Domain.Enums;

namespace EstudioContable.Application.DTOs.Auth;

public record LoginRequestDto(string Email, string Password);

public record AuthResponseDto(
    string Token,
    Guid UsuarioId,
    string NombreCompleto,
    string Email,
    RolUsuario Rol,
    Guid EstudioId,
    string NombreEstudio,
    Guid? ClienteId
);

public record MeDto(
    Guid UsuarioId,
    string Nombre,
    string Apellido,
    string Email,
    RolUsuario Rol,
    Guid EstudioId,
    string NombreEstudio,
    Guid? ClienteId
);

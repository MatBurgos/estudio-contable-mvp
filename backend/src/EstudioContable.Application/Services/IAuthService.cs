using EstudioContable.Application.DTOs.Auth;

namespace EstudioContable.Application.Services;

public interface IAuthService
{
    Task<AuthResponseDto?> LoginAsync(LoginRequestDto request, CancellationToken cancellationToken = default);
    Task<MeDto?> GetCurrentUserAsync(CancellationToken cancellationToken = default);
}

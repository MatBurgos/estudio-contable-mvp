using EstudioContable.Application.DTOs.Clientes;

namespace EstudioContable.Application.Services;

public interface IClienteService
{
    Task<List<ClienteListDto>> GetClientesAsync(string? search = null, bool soloActivos = true, CancellationToken cancellationToken = default);
    Task<ClienteDetailDto?> GetClienteByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<ClienteDetailDto> CreateClienteAsync(CreateClienteDto dto, CancellationToken cancellationToken = default);
    Task<ClienteDetailDto?> UpdateClienteAsync(Guid id, UpdateClienteDto dto, CancellationToken cancellationToken = default);
    Task<bool> DeleteClienteAsync(Guid id, CancellationToken cancellationToken = default);
    Task<bool> AsignarObligacionAsync(Guid clienteId, AsignarObligacionDto dto, CancellationToken cancellationToken = default);
    Task<bool> DesasignarObligacionAsync(Guid clienteId, Guid clienteObligacionId, CancellationToken cancellationToken = default);
}

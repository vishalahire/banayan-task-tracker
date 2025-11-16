using TaskTracker.Application.Commands;
using TaskTracker.Application.DTOs;

namespace TaskTracker.Application.Interfaces;

public interface IAuthService
{
    Task<AuthenticationResult> LoginAsync(LoginCommand command, CancellationToken ct = default);
    Task<AuthenticationResult> RegisterAsync(RegisterCommand command, CancellationToken ct = default);
    Task<bool> ChangePasswordAsync(ChangePasswordCommand command, CancellationToken ct = default);
    Task<UserDto?> GetUserByIdAsync(Guid userId, CancellationToken ct = default);
}
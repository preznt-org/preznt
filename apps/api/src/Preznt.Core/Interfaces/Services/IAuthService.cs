namespace Preznt.Core.Interfaces.Services;

using Preznt.Core.Common;

public interface IAuthService
{
    string GetGitHubLoginUrl(string? returnUrl = null);
    Task<Result<AuthResponse>> HandleGitHubCallbackAsync(string code, string state, CancellationToken ct = default);
    Task<Result<UserInfo>> GetCurrentUserAsync(Guid userId, CancellationToken ct = default);
}

public sealed record AuthResponse(
    string Token,
    DateTime ExpiresAt,
    UserInfo User);

public sealed record UserInfo(
    Guid Id,
    string Username,
    string? Email,
    string? Name,
    string? AvatarUrl);
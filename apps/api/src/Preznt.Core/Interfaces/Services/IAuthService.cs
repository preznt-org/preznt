namespace Preznt.Core.Interfaces.Services;

using Preznt.Core.Common;

public interface IAuthService
{
    string GetGitHubLoginUrl(string? returnUrl = null);
    Task<Result<AuthResponse>> HandleGitHubCallbackAsync(string code, string state, CancellationToken ct = default);
    Task<Result<AuthResponse>> RefreshTokenAsync(string refreshToken, CancellationToken ct = default);
    Task<Result<bool>> LogoutAsync(Guid userId, CancellationToken ct = default);
    Task<Result<MeResponse>> GetCurrentUserAsync(Guid userId, int page = 1, int pageSize = 10, CancellationToken ct = default);
}

public sealed record AuthResponse(
    string AccessToken,
    string RefreshToken,
    DateTime ExpiresAt,
    DateTime RefreshExpiresAt,
    UserInfo User);

public sealed record UserInfo(
    Guid Id,
    string Username,
    string? Email,
    string? Name,
    string? AvatarUrl);

public sealed record MeResponse(
    Guid Id,
    string Username,
    string? Email,
    string? Name,
    string? AvatarUrl,
    string? Bio,
    IReadOnlyList<RepositoryInfo> Repositories,
    PaginationInfo Pagination);

public sealed record RepositoryInfo(
    long Id,
    string Name,
    string? Description,
    string Url,
    string? Language,
    int Stars,
    int Forks,
    DateTime UpdatedAt);

public sealed record PaginationInfo(
    int Page,
    int PageSize,
    int TotalCount,
    int TotalPages,
    bool HasPreviousPage,
    bool HasNextPage);
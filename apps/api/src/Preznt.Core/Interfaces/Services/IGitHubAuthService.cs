namespace Preznt.Core.Interfaces.Services;

public interface IGitHubAuthService
{
    string GetAuthorizationUrl(string state);
    Task<GitHubTokenResponse?> ExchangeCodeForTokenAsync(string code, CancellationToken ct = default);
    Task<GitHubUserInfo?> GetUserInfoAsync(string accessToken, CancellationToken ct = default);
}

public sealed record GitHubTokenResponse(
    string AccessToken,
    string TokenType,
    string Scope);

public sealed record GitHubUserInfo(
    long Id,
    string Login,
    string? Email,
    string? Name,
    string? AvatarUrl);
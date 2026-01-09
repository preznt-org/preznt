using System.Text.Json.Serialization;

namespace Preznt.Core.Interfaces.Services;

public interface IGitHubAuthService
{
    string GetAuthorizationUrl(string state);
    Task<GitHubTokenResponse?> ExchangeCodeForTokenAsync(string code, CancellationToken ct = default);
    Task<GitHubUserInfo?> GetUserInfoAsync(string accessToken, CancellationToken ct = default);
}

public sealed record GitHubTokenResponse(
    [property: JsonPropertyName("access_token")] string AccessToken,
    [property: JsonPropertyName("token_type")] string TokenType,
    [property: JsonPropertyName("scope")] string Scope);

public sealed record GitHubUserInfo(
    [property: JsonPropertyName("id")] long Id,
    [property: JsonPropertyName("login")] string Login,
    [property: JsonPropertyName("email")] string? Email,
    [property: JsonPropertyName("name")] string? Name,
    [property: JsonPropertyName("avatar_url")] string? AvatarUrl);
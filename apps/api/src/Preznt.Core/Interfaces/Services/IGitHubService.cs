namespace Preznt.Core.Interfaces.Services;

public interface IGitHubService
{
    string GetAuthorizationUrl(string state);
    Task<GitHubTokenResult?> ExchangeCodeForTokenAsync(string code, CancellationToken ct = default);
    Task<GitHubUserResult?> GetUserAsync(string accessToken, CancellationToken ct = default);
    Task<IReadOnlyList<GitHubRepoResult>> GetUserRepositoriesAsync(string accessToken, CancellationToken ct = default);
}

public sealed record GitHubTokenResult(string AccessToken);

public sealed record GitHubUserResult(
    long Id,
    string Login,
    string? Email,
    string? Name,
    string? AvatarUrl,
    string? Bio);

public sealed record GitHubRepoResult(
    long Id,
    string Name,
    string? Description,
    string HtmlUrl,
    string? Language,
    int Stars,
    int Forks,
    DateTime UpdatedAt);

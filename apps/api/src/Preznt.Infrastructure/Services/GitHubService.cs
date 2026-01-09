using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Octokit;
using Preznt.Core.Interfaces.Services;
using Preznt.Core.Settings;

namespace Preznt.Infrastructure.Services;

public sealed class GitHubService : IGitHubService
{
    private readonly GitHubSettings _settings;
    private readonly ILogger<GitHubService> _logger;

    public GitHubService(
        IOptions<GitHubSettings> settings,
        ILogger<GitHubService> logger)
    {
        _settings = settings.Value;
        _logger = logger;
    }

    public string GetAuthorizationUrl(string state)
    {
        var client = CreateClient();
        var request = new OauthLoginRequest(_settings.ClientId)
        {
            State = state,
            RedirectUri = new Uri(_settings.CallbackUrl)
        };

        foreach (var scope in _settings.Scopes)
        {
            request.Scopes.Add(scope);
        }

        return client.Oauth.GetGitHubLoginUrl(request).ToString();
    }

    public async Task<GitHubTokenResult?> ExchangeCodeForTokenAsync(string code, CancellationToken ct = default)
    {
        try
        {
            var client = CreateClient();
            var request = new OauthTokenRequest(_settings.ClientId, _settings.ClientSecret, code);
            var token = await client.Oauth.CreateAccessToken(request);

            if (string.IsNullOrEmpty(token.AccessToken))
            {
                _logger.LogError("GitHub Service - OAuth response missing access token");
                return null;
            }

            return new GitHubTokenResult(token.AccessToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "GitHub Service - Error exchanging code for token");
            return null;
        }
    }

    public async Task<GitHubUserResult?> GetUserAsync(string accessToken, CancellationToken ct = default)
    {
        try
        {
            var client = CreateAuthenticatedClient(accessToken);
            var user = await client.User.Current();

            // Get primary email if not public
            string? email = user.Email;
            if (string.IsNullOrEmpty(email))
            {
                try
                {
                    var emails = await client.User.Email.GetAll();
                    var primary = emails.FirstOrDefault(e => e.Primary && e.Verified);
                    email = primary?.Email;
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "GitHub Service - Could not fetch user emails");
                }
            }

            return new GitHubUserResult(
                user.Id,
                user.Login,
                email,
                user.Name,
                user.AvatarUrl,
                user.Bio);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "GitHub Service - Error fetching user info");
            return null;
        }
    }

    public async Task<IReadOnlyList<GitHubRepoResult>> GetUserRepositoriesAsync(string accessToken, CancellationToken ct = default)
    {
        try
        {
            var client = CreateAuthenticatedClient(accessToken);
            var user = await client.User.Current();
            var repos = await client.Repository.GetAllForCurrent(new RepositoryRequest
            {
                Sort = RepositorySort.Updated,
                Direction = SortDirection.Descending
            });

            return repos
                .Where(r => !r.Fork && r.Owner.Login == user.Login) // Exclude forks and repos where user is only a collaborator
                .Select(r => new GitHubRepoResult(
                    r.Id,
                    r.Name,
                    r.Description,
                    r.HtmlUrl,
                    r.Language,
                    r.StargazersCount,
                    r.ForksCount,
                    r.UpdatedAt.UtcDateTime
                )).ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "GitHub Service - Error fetching repositories");
            return [];
        }
    }

    private GitHubClient CreateClient()
    {
        return new GitHubClient(new ProductHeaderValue("Preznt"));
    }

    private GitHubClient CreateAuthenticatedClient(string accessToken)
    {
        var client = CreateClient();
        client.Credentials = new Credentials(accessToken);
        return client;
    }
}

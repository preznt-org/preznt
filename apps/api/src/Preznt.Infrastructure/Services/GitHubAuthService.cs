using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Preznt.Core.Constants;
using Preznt.Core.Interfaces.Services;
using Preznt.Core.Settings;

namespace Preznt.Infrastructure.Services;

public sealed class GitHubAuthService : IGitHubAuthService
{
    private readonly HttpClient _httpClient;
    private readonly GitHubSettings _settings;
    private readonly ILogger<GitHubAuthService> _logger;

    public GitHubAuthService(
        HttpClient httpClient,
        IOptions<GitHubSettings> settings,
        ILogger<GitHubAuthService> logger)
    {
        _httpClient = httpClient;
        _settings = settings.Value;
        _logger = logger;
    }

    public string GetAuthorizationUrl(string state)
    {
        var scopes = string.Join(" ", _settings.Scopes);
        return $"{Constants.AuthorizationEndpoint}?" +
               $"client_id={_settings.ClientId}" +
               $"$redirect_uri={Uri.EscapeDataString(_settings.CallbackUrl)}" +
               $"$scope={Uri.EscapeDataString(scopes)}" +
               $"$state={state}";
    }

    public async Task<GitHubTokenResponse?> ExchangeCodeForTokenAsync(string code, CancellationToken ct = default)
    {
        var request = new HttpRequestMessage(HttpMethod.Post, Constants.TokenEndpoint)
        {
            Content = JsonContent.Create(new
            {
                client_id = _settings.ClientId,
                client_secret = _settings.ClientSecret,
                code = code,
                redirect_uri = _settings.CallbackUrl
            })
        };

        request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

        try
        {
            var response = await _httpClient.SendAsync(request, ct);
            response.EnsureSuccessStatusCode();

            var tokenResponse = await response.Content.ReadFromJsonAsync<GitHubTokenResponse>(cancellationToken: ct);

            if (tokenResponse?.AccessToken is null)
            {
                _logger.LogError("GitHub Auth Service - GitHub OAuth response missing access token.");
                return null;
            }

            return new GitHubTokenResponse(
                AccessToken: tokenResponse.AccessToken,
                TokenType: tokenResponse.TokenType ?? "bearer",
                Scope: tokenResponse.Scope ?? "");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "GitHub Auth Service - Error exchanging code for token.");
            return null;
        }
    }

    public async Task<GitHubUserInfo?> GetUserInfoAsync(string accessToken, CancellationToken ct = default)
    {
        var request = new HttpRequestMessage(HttpMethod.Get, $"{Constants.UserEndpoint}");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
        request.Headers.UserAgent.Add(new ProductInfoHeaderValue("PrezntApp", "1.0"));

        try
        {
            var response = await _httpClient.SendAsync(request, ct);
            response.EnsureSuccessStatusCode();

            var userResponse = await response.Content.ReadFromJsonAsync<GitHubUserInfo>(cancellationToken: ct);

            if (userResponse is null)
            {
                _logger.LogError("GitHub Auth Service - GitHub user info response is null.");
                return null;
            }

            // Get primary email if not public
            var email = userResponse.Email;
            if (string.IsNullOrEmpty(email))
            {
                email = await GetPrimaryEmailAsync(accessToken, ct);
            }

            return new GitHubUserInfo(
                userResponse.Id,
                userResponse.Login,
                email,
                userResponse.Name,
                userResponse.AvatarUrl
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "GitHub Auth Service - Error fetching user info.");
            return null;
        }
    }

    private async Task<string?> GetPrimaryEmailAsync(string accessToken, CancellationToken ct = default)
    {
        var request = new HttpRequestMessage(HttpMethod.Get, $"{Constants.UserEmailsEndpoint}");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
        request.Headers.UserAgent.Add(new ProductInfoHeaderValue("PrezntApp", "1.0"));

        try
        {
            var response = await _httpClient.SendAsync(request, ct);
            response.EnsureSuccessStatusCode();

            var emails = await response.Content.ReadFromJsonAsync<List<GitHubEmailResponse>>(cancellationToken: ct);

            var primaryEmail = emails?.FirstOrDefault(e => e.Primary && e.Verified);
            return primaryEmail?.Email;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "GitHub Auth Service - Error fetching user emails.");
            return null;
        }
    }

    // Internal DTOs for GitHub API responses
    private sealed record GitHubOAuthResponse(
        [property: JsonPropertyName("access_token")] string? AccessToken,
        [property: JsonPropertyName("token_type")] string? TokenType,
        [property: JsonPropertyName("scope")] string? Scope);

    private sealed record GitHubUserResponse(
        [property: JsonPropertyName("id")] long Id,
        [property: JsonPropertyName("login")] string Login,
        [property: JsonPropertyName("email")] string? Email,
        [property: JsonPropertyName("name")] string? Name,
        [property: JsonPropertyName("avatar_url")] string? AvatarUrl);

    private sealed record GitHubEmailResponse(
        [property: JsonPropertyName("email")] string Email,
        [property: JsonPropertyName("primary")] bool Primary,
        [property: JsonPropertyName("verified")] bool Verified);
}
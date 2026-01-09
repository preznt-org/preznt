namespace Preznt.Infrastructure.Services;

using Microsoft.Extensions.Logging;
using Preznt.Core.Common;
using Preznt.Core.Entities;
using Preznt.Core.Interfaces.Repositories;
using Preznt.Core.Interfaces.Services;

public sealed class AuthService : IAuthService
{
    private readonly IGitHubAuthService _gitHubAuthService;
    private readonly IUserRepository _userRepository;
    private readonly IJwtService _jwtService;
    private readonly ILogger<AuthService> _logger;

    public AuthService(
        IGitHubAuthService gitHubAuthService,
        IUserRepository userRepository,
        IJwtService jwtService,
        ILogger<AuthService> logger)
    {
        _gitHubAuthService = gitHubAuthService;
        _userRepository = userRepository;
        _jwtService = jwtService;
        _logger = logger;
    }

    public string GetGitHubLoginUrl(string? returnUrl = null)
    {
        var state = Convert.ToBase64String(Guid.NewGuid().ToByteArray());
        // TODO: Store state with returnUrl in cache for validation
        return _gitHubAuthService.GetAuthorizationUrl(state);
    }

    public async Task<Result<AuthResponse>> HandleGitHubCallbackAsync(
        string code, 
        string state, 
        CancellationToken ct = default)
    {
        // TODO: Validate state from cache

        // Exchange code for token
        var tokenResponse = await _gitHubAuthService.ExchangeCodeForTokenAsync(code, ct);
        if (tokenResponse is null)
        {
            _logger.LogWarning("Auth Service - Failed to exchange GitHub code for token");
            return Result<AuthResponse>.Failure(
                new ResultError(ErrorType.Unauthorized, "Failed to authenticate with GitHub"));
        }

        // Get user info from GitHub
        var gitHubUser = await _gitHubAuthService.GetUserInfoAsync(tokenResponse.AccessToken, ct);
        if (gitHubUser is null)
        {
            _logger.LogWarning("Auth Service - Failed to get GitHub user info");
            return Result<AuthResponse>.Failure(
                new ResultError(ErrorType.Unauthorized, "Failed to get user information from GitHub"));
        }

        // Find or create user
        var user = await _userRepository.GetByGitHubIdAsync(gitHubUser.Id, ct);
        
        if (user is null)
        {
            user = User.Create(
                gitHubUser.Id,
                gitHubUser.Login,
                gitHubUser.Email,
                gitHubUser.Name,
                gitHubUser.AvatarUrl,
                tokenResponse.AccessToken);
            
            _userRepository.Add(user);
            _logger.LogInformation("Created new user {Username} with GitHub ID {GitHubId}", 
                gitHubUser.Login, gitHubUser.Id);
        }
        else
        {
            user.UpdateFromGitHub(
                gitHubUser.Login,
                gitHubUser.Email,
                gitHubUser.Name,
                gitHubUser.AvatarUrl,
                tokenResponse.AccessToken);
            
            _userRepository.Update(user);
            _logger.LogInformation("Updated existing user {Username}", gitHubUser.Login);
        }

        await _userRepository.SaveChangesAsync(ct);

        // Generate JWT
        var jwt = _jwtService.GenerateToken(user);
        var expiresAt = _jwtService.GetExpiryDate();

        return Result<AuthResponse>.Success(new AuthResponse(
            jwt,
            expiresAt,
            new UserInfo(
                user.Id,
                user.Username,
                user.Email,
                user.Name,
                user.AvatarUrl)));
    }

    public async Task<Result<UserInfo>> GetCurrentUserAsync(Guid userId, CancellationToken ct = default)
    {
        var user = await _userRepository.GetByIdAsync(userId, ct);
        
        if (user is null)
        {
            return Result<UserInfo>.Failure(
                new ResultError(ErrorType.NotFound, "User not found"));
        }

        return Result<UserInfo>.Success(new UserInfo(
            user.Id,
            user.Username,
            user.Email,
            user.Name,
            user.AvatarUrl));
    }
}
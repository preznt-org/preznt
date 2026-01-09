namespace Preznt.Infrastructure.Services;

using System.Security.Cryptography;
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
        return _gitHubAuthService.GetAuthorizationUrl(state);
    }

    public async Task<Result<AuthResponse>> HandleGitHubCallbackAsync(
        string code, 
        string state, 
        CancellationToken ct = default)
    {
        var tokenResponse = await _gitHubAuthService.ExchangeCodeForTokenAsync(code, ct);
        if (tokenResponse is null)
        {
            _logger.LogWarning("Failed to exchange GitHub code for token");
            return Result<AuthResponse>.Failure(
                new ResultError(ErrorType.Unauthorized, "Failed to authenticate with GitHub"));
        }

        var gitHubUser = await _gitHubAuthService.GetUserInfoAsync(tokenResponse.AccessToken, ct);
        if (gitHubUser is null)
        {
            _logger.LogWarning("Failed to get GitHub user info");
            return Result<AuthResponse>.Failure(
                new ResultError(ErrorType.Unauthorized, "Failed to get user information from GitHub"));
        }

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
            _logger.LogInformation("Created new user {Username}", gitHubUser.Login);
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
        }

        return await GenerateTokensAndSave(user, ct);
    }

    public async Task<Result<AuthResponse>> RefreshTokenAsync(string refreshToken, CancellationToken ct = default)
    {
        var hash = HashToken(refreshToken);
        var user = await _userRepository.GetByRefreshTokenHashAsync(hash, ct);

        if (user is null || !user.ValidateRefreshToken(hash))
        {
            return Result<AuthResponse>.Failure(
                new ResultError(ErrorType.Unauthorized, "Invalid refresh token"));
        }

        return await GenerateTokensAndSave(user, ct);
    }

    public async Task<Result<bool>> LogoutAsync(Guid userId, CancellationToken ct = default)
    {
        var user = await _userRepository.GetByIdAsync(userId, ct);
        if (user is null)
        {
            return Result<bool>.Failure(new ResultError(ErrorType.NotFound, "User not found"));
        }

        user.ClearRefreshToken();
        _userRepository.Update(user);
        await _userRepository.SaveChangesAsync(ct);

        return Result<bool>.Success(true);
    }

    public async Task<Result<UserInfo>> GetCurrentUserAsync(Guid userId, CancellationToken ct = default)
    {
        var user = await _userRepository.GetByIdAsync(userId, ct);
        
        if (user is null)
        {
            return Result<UserInfo>.Failure(new ResultError(ErrorType.NotFound, "User not found"));
        }

        return Result<UserInfo>.Success(new UserInfo(
            user.Id, user.Username, user.Email, user.Name, user.AvatarUrl));
    }

    private async Task<Result<AuthResponse>> GenerateTokensAndSave(User user, CancellationToken ct)
    {
        var accessToken = _jwtService.GenerateAccessToken(user);
        var (refreshToken, refreshHash, refreshExpiry) = _jwtService.GenerateRefreshToken();
        
        user.SetRefreshToken(refreshHash, refreshExpiry);
        _userRepository.Update(user);
        await _userRepository.SaveChangesAsync(ct);

        return Result<AuthResponse>.Success(new AuthResponse(
            accessToken,
            refreshToken,
            _jwtService.GetAccessTokenExpiry(),
            refreshExpiry,
            new UserInfo(user.Id, user.Username, user.Email, user.Name, user.AvatarUrl)));
    }

    private static string HashToken(string token) =>
        Convert.ToBase64String(SHA256.HashData(System.Text.Encoding.UTF8.GetBytes(token)));
}
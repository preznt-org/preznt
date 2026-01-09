using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using Preznt.Core.Common;
using Preznt.Core.Interfaces.Services;

namespace Preznt.Api.Endpoints;

public static class AuthEndpoints
{
    private const string RefreshTokenCookie = "refresh_token";

    public static RouteGroupBuilder MapAuthEndpoints(this RouteGroupBuilder group)
    {
        group.MapGet("/login", Login)
            .WithName("Login")
            .WithSummary("Initiates GitHub OAuth login");

        group.MapGet("/callback", Callback)
            .WithName("OAuthCallback")
            .WithSummary("Handles GitHub OAuth callback");
        
        group.MapPost("/refresh", Refresh)
            .WithName("RefreshToken")
            .WithSummary("Exchanges refresh token for new token pair");

        group.MapPost("/logout", Logout)
            .WithName("Logout")
            .WithSummary("Revokes refresh token")
            .RequireAuthorization();
        
        group.MapGet("/me", GetCurrentUser)
            .WithName("GetCurrentUser")
            .WithSummary("Gets the authenticated user with their GitHub repositories")
            .RequireAuthorization();

        return group;
    }

    private static IResult Login(IAuthService authService, string? returnUrl = null)
    {
        return Results.Redirect(authService.GetGitHubLoginUrl(returnUrl));
    }

    private static async Task<IResult> Callback(
        HttpContext context,
        IAuthService authService,
        [FromQuery] string code,
        [FromQuery] string state,
        IConfiguration configuration,
        CancellationToken ct)
    {
        var result = await authService.HandleGitHubCallbackAsync(code, state, ct);
        
        if (result.IsFailure)
            return ToProblem(result.Error!);

        var auth = result.Value!;
        SetRefreshTokenCookie(context, auth.RefreshToken, auth.RefreshExpiresAt);

        var frontendUrl = configuration["Frontend:BaseUrl"] ?? "http://localhost:3000";
        return Results.Redirect($"{frontendUrl}/auth/callback?access_token={auth.AccessToken}");
    }

    private static async Task<IResult> Refresh(
        HttpContext context,
        IAuthService authService,
        CancellationToken ct)
    {
        var refreshToken = context.Request.Cookies[RefreshTokenCookie];
        
        if (string.IsNullOrEmpty(refreshToken))
            return Results.Problem("Refresh token not found", statusCode: 401);

        var result = await authService.RefreshTokenAsync(refreshToken, ct);
        
        if (result.IsFailure)
        {
            ClearRefreshTokenCookie(context);
            return ToProblem(result.Error!);
        }

        var auth = result.Value!;
        SetRefreshTokenCookie(context, auth.RefreshToken, auth.RefreshExpiresAt);

        return Results.Ok(new { auth.AccessToken, auth.ExpiresAt, auth.User });
    }

    private static async Task<IResult> Logout(
        HttpContext context,
        IAuthService authService,
        CancellationToken ct)
    {
        var userId = GetUserId(context);
        if (userId is null)
            return Results.Unauthorized();

        await authService.LogoutAsync(userId.Value, ct);
        ClearRefreshTokenCookie(context);
        
        return Results.NoContent();
    }

    private static async Task<IResult> GetCurrentUser(
        HttpContext context,
        IAuthService authService,
        CancellationToken ct = default)
    {
        var userId = GetUserId(context);
        if (userId is null)
            return Results.Unauthorized();

        var page = GetHeaderInt(context, "X-Page", 1);
        var pageSize = GetHeaderInt(context, "X-Page-Size", 10);

        var result = await authService.GetCurrentUserAsync(userId.Value, page, pageSize, ct);

        if (result.IsFailure)
            return ToProblem(result.Error!);

        return Results.Ok(result.Value);
    }

    private static int GetHeaderInt(HttpContext context, string headerName, int defaultValue)
    {
        if (context.Request.Headers.TryGetValue(headerName, out var value) &&
            int.TryParse(value, out var parsed))
        {
            return parsed;
        }
        return defaultValue;
    }

    private static void SetRefreshTokenCookie(HttpContext context, string token, DateTime expiresAt)
    {
        context.Response.Cookies.Append(RefreshTokenCookie, token, new CookieOptions
        {
            HttpOnly = true,
            Secure = true,
            SameSite = SameSiteMode.Strict,
            Expires = expiresAt,
            Path = "/api/auth"
        });
    }

    private static void ClearRefreshTokenCookie(HttpContext context)
    {
        context.Response.Cookies.Delete(RefreshTokenCookie, new CookieOptions
        {
            HttpOnly = true,
            Secure = true,
            SameSite = SameSiteMode.Strict,
            Path = "/api/auth"
        });
    }

    private static Guid? GetUserId(HttpContext context)
    {
        var claim = context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return Guid.TryParse(claim, out var id) ? id : null;
    }

    private static IResult ToProblem(ResultError error) => error.Type switch
    {
        ErrorType.NotFound => Results.Problem(error.Message, statusCode: 404),
        ErrorType.Validation => Results.Problem(error.Message, statusCode: 400),
        ErrorType.Unauthorized => Results.Problem(error.Message, statusCode: 401),
        ErrorType.Forbidden => Results.Problem(error.Message, statusCode: 403),
        ErrorType.Conflict => Results.Problem(error.Message, statusCode: 409),
        _ => Results.Problem("An unexpected error occurred", statusCode: 500)
    };
}
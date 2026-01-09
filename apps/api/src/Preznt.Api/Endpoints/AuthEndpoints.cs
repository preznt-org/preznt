using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using Preznt.Core.Common;
using Preznt.Core.Interfaces.Services;

namespace Preznt.Api.Endpoints;

public static class AuthEndpoints
{
    public static RouteGroupBuilder MapAuthEndpoints(this RouteGroupBuilder group)
    {
        group.MapGet("/login", Login)
            .WithName("Login")
            .WithSummary("Initiates GitHub OAuth login")
            .Produces<RedirectResult>(302);

        group.MapGet("/callback", Callback)
            .WithName("OAuthCallback")
            .WithSummary("Handles GitHub OAuth callback")
            .Produces(200)
            .Produces(401);
        
        group.MapGet("/me", GetCurrentUser)
            .WithName("GetCurrentUser")
            .WithSummary("Gets the currently authenticated user")
            .RequireAuthorization()
            .Produces(200)
            .Produces(401);

        return group;
    }

    private static IResult Login(
        IAuthService authSerivce,
        string? returnUrl = null)
    {
        var loginUrl = authSerivce.GetGitHubLoginUrl(returnUrl);
        return Results.Redirect(loginUrl);   
    }

    private static async Task<IResult> Callback(
        IAuthService authService,
        [FromQuery] string code,
        [FromQuery] string state,
        IConfiguration configuration,
        CancellationToken ct)
    {
        var result = await authService.HandleGitHubCallbackAsync(code, state, ct);
        
        if (result.IsFailure)
        {
            return result.Error!.Type switch
            {
                ErrorType.Unauthorized => Results.Problem(
                    title: "Authentication Failed",
                    detail: result.Error.Message,
                    statusCode: StatusCodes.Status401Unauthorized),
                _ => Results.Problem(
                    title: "Authentication Error",
                    detail: result.Error.Message,
                    statusCode: StatusCodes.Status500InternalServerError)
            };
        }

        var frontendUrl = configuration["Frontend:BaseUrl"] ?? "http://localhost:3000";
        var redirectUrl = $"{frontendUrl}/auth/callback?token={result.Value!.Token}";
        return Results.Redirect(redirectUrl);
    }

    private static async Task<IResult> GetCurrentUser(
        HttpContext context,
        IAuthService authService,
        CancellationToken ct)
    {
        var userIdClaim = context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (userIdClaim is null || !Guid.TryParse(userIdClaim, out var userId))
        {
            return Results.Problem(
                title: "Invalid Token",
                detail: "Could not extract user information from token",
                statusCode: 401);
        }

        var result = await authService.GetCurrentUserAsync(userId, ct);
        
        if (result.IsFailure)
        {
            return result.Error!.Type switch
            {
                ErrorType.NotFound => Results.Problem(
                    title: "User Not Found",
                    detail: result.Error.Message,
                    statusCode: StatusCodes.Status404NotFound),
                _ => Results.Problem(
                    title: "Error",
                    detail: result.Error.Message,
                    statusCode: StatusCodes.Status500InternalServerError)
            };
        }

        return Results.Ok(result.Value);
    }
}
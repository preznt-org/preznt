using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using Preznt.Core.Common;
using Preznt.Core.Interfaces.Services;

namespace Preznt.Api.Endpoints;

public static class PortfolioEndpoints
{
    public static RouteGroupBuilder MapPortfolioEndpoints(this RouteGroupBuilder group)
    {
        group.MapPost("/", CreatePortfolio)
            .WithName("CreatePortfolio")
            .WithSummary("Create a new portfolio")
            .Produces<PortfolioResponse>(StatusCodes.Status201Created)
            .ProducesValidationProblem();

        group.MapGet("/", GetAllPortfolios)
            .WithName("GetAllPortfolios")
            .WithSummary("Get all portfolios for the current user")
            .Produces<IReadOnlyList<PortfolioListItem>>();

        group.MapGet("/{id:guid}", GetPortfolioById)
            .WithName("GetPortfolioById")
            .WithSummary("Get a portfolio by ID")
            .Produces<PortfolioResponse>()
            .ProducesProblem(StatusCodes.Status404NotFound);

        group.MapPut("/{id:guid}", UpdatePortfolio)
            .WithName("UpdatePortfolio")
            .WithSummary("Update a portfolio")
            .Produces<PortfolioResponse>()
            .ProducesProblem(StatusCodes.Status404NotFound);

        group.MapDelete("/{id:guid}", DeletePortfolio)
            .WithName("DeletePortfolio")
            .WithSummary("Delete a portfolio")
            .Produces(StatusCodes.Status204NoContent)
            .ProducesProblem(StatusCodes.Status404NotFound);

        group.MapPut("/{id:guid}/projects", SetProjects)
            .WithName("SetPortfolioProjects")
            .WithSummary("Set projects for a portfolio")
            .Produces<PortfolioResponse>()
            .ProducesProblem(StatusCodes.Status404NotFound);

        group.MapPut("/{id:guid}/skills", SetSkills)
            .WithName("SetPortfolioSkills")
            .WithSummary("Set skills for a portfolio")
            .Produces<PortfolioResponse>()
            .ProducesProblem(StatusCodes.Status404NotFound);

        return group;
    }

    private static async Task<IResult> CreatePortfolio(
        HttpContext context,
        IPortfolioService portfolioService,
        CreatePortfolioRequest request,
        CancellationToken ct)
    {
        var userId = GetUserId(context);
        if (userId is null) return Results.Unauthorized();

        var result = await portfolioService.CreateAsync(userId.Value, request, ct);

        return result.IsSuccess
            ? Results.Created($"/api/portfolios/{result.Value!.Id}", result.Value)
            : ToProblem(result.Error!);
    }

    private static async Task<IResult> GetAllPortfolios(
        HttpContext context,
        IPortfolioService portfolioService,
        CancellationToken ct)
    {
        var userId = GetUserId(context);
        if (userId is null) return Results.Unauthorized();

        var result = await portfolioService.GetAllAsync(userId.Value, ct);

        return result.IsSuccess
            ? Results.Ok(result.Value)
            : ToProblem(result.Error!);
    }

    private static async Task<IResult> GetPortfolioById(
        HttpContext context,
        IPortfolioService portfolioService,
        Guid id,
        CancellationToken ct)
    {
        var userId = GetUserId(context);
        if (userId is null) return Results.Unauthorized();

        var result = await portfolioService.GetByIdAsync(userId.Value, id, ct);

        return result.IsSuccess
            ? Results.Ok(result.Value)
            : ToProblem(result.Error!);
    }

    private static async Task<IResult> UpdatePortfolio(
        HttpContext context,
        IPortfolioService portfolioService,
        Guid id,
        UpdatePortfolioRequest request,
        CancellationToken ct)
    {
        var userId = GetUserId(context);
        if (userId is null) return Results.Unauthorized();

        var result = await portfolioService.UpdateAsync(userId.Value, id, request, ct);

        return result.IsSuccess
            ? Results.Ok(result.Value)
            : ToProblem(result.Error!);
    }

    private static async Task<IResult> DeletePortfolio(
        HttpContext context,
        IPortfolioService portfolioService,
        Guid id,
        CancellationToken ct)
    {
        var userId = GetUserId(context);
        if (userId is null) return Results.Unauthorized();

        var result = await portfolioService.DeleteAsync(userId.Value, id, ct);

        return result.IsSuccess
            ? Results.NoContent()
            : ToProblem(result.Error!);
    }

    private static async Task<IResult> SetProjects(
        HttpContext context,
        IPortfolioService portfolioService,
        Guid id,
        SetProjectsRequest request,
        CancellationToken ct)
    {
        var userId = GetUserId(context);
        if (userId is null) return Results.Unauthorized();

        var result = await portfolioService.SetProjectsAsync(userId.Value, id, request, ct);

        return result.IsSuccess
            ? Results.Ok(result.Value)
            : ToProblem(result.Error!);
    }

    private static async Task<IResult> SetSkills(
        HttpContext context,
        IPortfolioService portfolioService,
        Guid id,
        SetSkillsRequest request,
        CancellationToken ct)
    {
        var userId = GetUserId(context);
        if (userId is null) return Results.Unauthorized();

        var result = await portfolioService.SetSkillsAsync(userId.Value, id, request, ct);

        return result.IsSuccess
            ? Results.Ok(result.Value)
            : ToProblem(result.Error!);
    }

    private static Guid? GetUserId(HttpContext context)
    {
        var claim = context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return Guid.TryParse(claim, out var id) ? id : null;
    }

    private static IResult ToProblem(ResultError error) => error.Type switch
    {
        ErrorType.NotFound => Results.Problem(error.Message, statusCode: StatusCodes.Status404NotFound),
        ErrorType.Validation => Results.Problem(error.Message, statusCode: StatusCodes.Status400BadRequest),
        ErrorType.Unauthorized => Results.Problem(error.Message, statusCode: StatusCodes.Status401Unauthorized),
        ErrorType.Forbidden => Results.Problem(error.Message, statusCode: StatusCodes.Status403Forbidden),
        ErrorType.Conflict => Results.Problem(error.Message, statusCode: StatusCodes.Status409Conflict),
        _ => Results.Problem("An unexpected error occurred", statusCode: StatusCodes.Status500InternalServerError)
    };
}
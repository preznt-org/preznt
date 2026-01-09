using Preznt.Core.Common;
using Preznt.Core.Interfaces.Services;

namespace Preznt.Api.Endpoints;

public static class ThemeEndpoints
{
    public static RouteGroupBuilder MapThemeEndpoints(this RouteGroupBuilder group)
    {
        group.MapGet("/", GetAll)
            .WithName("GetThemes")
            .WithSummary("Get all available themes")
            .Produces<IReadOnlyList<Theme>>();

        group.MapGet("/{id}", GetById)
            .WithName("GetThemeById")
            .WithSummary("Get a theme by ID")
            .Produces<Theme>()
            .ProducesProblem(StatusCodes.Status404NotFound);

        group.MapGet("/{id}/preview", GetPreviewHtml)
            .WithName("GetThemePreview")
            .WithSummary("Get rendered HTML preview of theme with sample data")
            .Produces<string>(contentType: "text/html")
            .ProducesProblem(StatusCodes.Status404NotFound);

        return group;
    }

    private static IResult GetAll()
    {
        return Results.Ok(AvailableThemes.All);
    }

    private static IResult GetById(string id)
    {
        var theme = AvailableThemes.GetById(id);
        return theme is not null 
            ? Results.Ok(theme) 
            : Results.Problem($"Theme '{id}' not found", statusCode: StatusCodes.Status404NotFound);
    }

    private static async Task<IResult> GetPreviewHtml(
        string id,
        IThemeRenderService renderService,
        CancellationToken ct)
    {
        var result = await renderService.RenderPreviewAsync(id, ct);

        if (result.IsFailure)
        {
            return result.Error!.Type == Core.Common.ErrorType.NotFound
                ? Results.Problem(result.Error.Message, statusCode: StatusCodes.Status404NotFound)
                : Results.Problem(result.Error.Message, statusCode: StatusCodes.Status500InternalServerError);
        }

        return Results.Content(result.Value!, "text/html");
    }
}
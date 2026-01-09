using System.Reflection;
using Microsoft.Extensions.Logging;
using Preznt.Core.Common;
using Preznt.Core.Entities;
using Preznt.Core.Interfaces.Services;
using Scriban;
using Scriban.Runtime;

namespace Preznt.Infrastructure.Services;

public sealed class ThemeRenderService : IThemeRenderService
{
    private readonly ILogger<ThemeRenderService> _logger;
    private readonly Dictionary<string, Template> _templateCache = new();
    private readonly Dictionary<string, string> _cssCache = new();

    public ThemeRenderService(ILogger<ThemeRenderService> logger)
    {
        _logger = logger;
    }

    public async Task<Result<RenderedPortfolio>> RenderAsync(
        Portfolio portfolio,
        string themeId,
        CancellationToken ct = default)
    {
        if (!AvailableThemes.Exists(themeId))
        {
            return Result<RenderedPortfolio>.Failure(
                new ResultError(ErrorType.NotFound, $"Theme '{themeId}' not found"));
        }

        try
        {
            var template = await GetTemplateAsync(themeId);
            var css = await GetCssAsync(themeId);

            var model = CreateModel(
                displayName: portfolio.DisplayName,
                position: portfolio.Position,
                company: portfolio.Company,
                location: portfolio.Location,
                bio: portfolio.Bio,
                profileImageUrl: portfolio.ProfileImageUrl,
                contactEmail: portfolio.ContactEmail,
                githubUrl: portfolio.GitHubUrl,
                linkedinUrl: portfolio.LinkedInUrl,
                twitterUrl: portfolio.TwitterUrl,
                websiteUrl: portfolio.WebsiteUrl,
                projects: portfolio.Projects.OrderBy(x => x.DisplayOrder).Select(p => (
                    p.Name,
                    Description: (string?)(p.AiDescription ?? p.OriginalDescription),
                    RepoUrl: (string?)p.RepoUrl,
                    Language: (string?)p.Language,
                    p.Stars,
                    p.Forks,
                    Highlights: ParseHighlights(p.AiHighlights)
                )),
                skills: portfolio.Skills.OrderBy(x => x.DisplayOrder).Select(s => (
                    s.Name,
                    Category: s.Category.ToString().ToLowerInvariant()
                )),
                css: css);

            var html = await template.RenderAsync(model);

            var files = new List<RenderedFile>
            {
                new("index.html", html),
                new("styles.css", css)
            };

            return Result<RenderedPortfolio>.Success(new RenderedPortfolio(html, css, files));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to render portfolio {PortfolioId} with theme {ThemeId}", 
                portfolio.Id, themeId);
            return Result<RenderedPortfolio>.Failure(
                new ResultError(ErrorType.InternalError, "Failed to render portfolio"));
        }
    }

    public async Task<Result<string>> RenderPreviewAsync(string themeId, CancellationToken ct = default)
    {
        if (!AvailableThemes.Exists(themeId))
        {
            return Result<string>.Failure(
                new ResultError(ErrorType.NotFound, $"Theme '{themeId}' not found"));
        }

        try
        {
            _logger.LogDebug("Loading template for theme {ThemeId}", themeId);
            var template = await GetTemplateAsync(themeId);
            
            _logger.LogDebug("Loading CSS for theme {ThemeId}", themeId);
            var css = await GetCssAsync(themeId);

            _logger.LogDebug("Creating model for theme {ThemeId}", themeId);
            var sample = AvailableThemes.SampleData;
            var model = CreateModel(
                displayName: sample.DisplayName,
                position: sample.Position,
                company: sample.Company,
                location: sample.Location,
                bio: sample.Bio,
                profileImageUrl: sample.AvatarUrl,
                contactEmail: null,
                githubUrl: sample.GitHubUrl,
                linkedinUrl: sample.LinkedInUrl,
                twitterUrl: null,
                websiteUrl: null,
                projects: sample.Projects.Select(p => (
                    p.Name,
                    Description: (string?)p.Description,
                    RepoUrl: (string?)p.RepoUrl,
                    Language: (string?)p.Language,
                    p.Stars,
                    p.Forks,
                    Highlights: new ScriptArray()
                )),
                skills: sample.Skills.Select(s => (
                    s.Name,
                    Category: s.Category.ToLowerInvariant()
                )),
                css: css);
            
            _logger.LogDebug("Rendering template for theme {ThemeId}", themeId);
            var html = await template.RenderAsync(model);

            return Result<string>.Success(html);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to render preview for theme {ThemeId}: {Message}", themeId, ex.Message);
            return Result<string>.Failure(
                new ResultError(ErrorType.InternalError, $"Failed to render preview: {ex.Message}"));
        }
    }

    public async Task<Result<IReadOnlyList<RenderedFile>>> GetRenderedFilesAsync(
        Portfolio portfolio,
        string themeId,
        CancellationToken ct = default)
    {
        var renderResult = await RenderAsync(portfolio, themeId, ct);
        if (renderResult.IsFailure)
        {
            return Result<IReadOnlyList<RenderedFile>>.Failure(renderResult.Error!);
        }

        return Result<IReadOnlyList<RenderedFile>>.Success(renderResult.Value!.Files);
    }

    private async Task<Template> GetTemplateAsync(string themeId)
    {
        if (_templateCache.TryGetValue(themeId, out var cached))
        {
            return cached;
        }

        // Convert theme ID to resource name (hyphens become underscores in embedded resources)
        var resourceName = themeId.Replace("-", "_");
        var templateContent = await LoadEmbeddedResourceAsync($"Templates.{resourceName}.template.html");
        var template = Template.Parse(templateContent);

        if (template.HasErrors)
        {
            throw new InvalidOperationException(
                $"Template errors for {themeId}: {string.Join(", ", template.Messages)}");
        }

        _templateCache[themeId] = template;
        return template;
    }

    private async Task<string> GetCssAsync(string themeId)
    {
        if (_cssCache.TryGetValue(themeId, out var cached))
        {
            return cached;
        }

        // Convert theme ID to resource name (hyphens become underscores in embedded resources)
        var resourceName = themeId.Replace("-", "_");
        var css = await LoadEmbeddedResourceAsync($"Templates.{resourceName}.styles.css");
        _cssCache[themeId] = css;
        return css;
    }

    private static async Task<string> LoadEmbeddedResourceAsync(string resourcePath)
    {
        var assembly = Assembly.GetExecutingAssembly();
        var fullPath = $"Preznt.Infrastructure.{resourcePath}";

        using var stream = assembly.GetManifestResourceStream(fullPath);
        if (stream is null)
        {
            throw new FileNotFoundException($"Embedded resource not found: {fullPath}");
        }

        using var reader = new StreamReader(stream);
        return await reader.ReadToEndAsync();
    }

    private static ScriptObject CreateModel(
        string displayName,
        string? position,
        string? company,
        string? location,
        string? bio,
        string? profileImageUrl,
        string? contactEmail,
        string? githubUrl,
        string? linkedinUrl,
        string? twitterUrl,
        string? websiteUrl,
        IEnumerable<(string Name, string? Description, string? RepoUrl, string? Language, int Stars, int Forks, ScriptArray Highlights)> projects,
        IEnumerable<(string Name, string Category)> skills,
        string css)
    {
        var model = new ScriptObject();
        
        var portfolioObj = new ScriptObject
        {
            ["display_name"] = displayName,
            ["position"] = position,
            ["company"] = company,
            ["location"] = location,
            ["bio"] = bio,
            ["profile_image_url"] = profileImageUrl,
            ["contact_email"] = contactEmail,
            ["github_url"] = githubUrl,
            ["linkedin_url"] = linkedinUrl,
            ["twitter_url"] = twitterUrl,
            ["website_url"] = websiteUrl
        };

        var projectsArray = new ScriptArray();
        foreach (var p in projects)
        {
            var projectObj = new ScriptObject
            {
                ["name"] = p.Name,
                ["description"] = p.Description,
                ["repo_url"] = p.RepoUrl,
                ["language"] = p.Language,
                ["stars"] = p.Stars,
                ["forks"] = p.Forks,
                ["ai_highlights"] = p.Highlights
            };
            projectsArray.Add(projectObj);
        }
        portfolioObj["projects"] = projectsArray;

        var skillsArray = new ScriptArray();
        foreach (var s in skills)
        {
            var skillObj = new ScriptObject
            {
                ["name"] = s.Name,
                ["category"] = s.Category
            };
            skillsArray.Add(skillObj);
        }
        portfolioObj["skills"] = skillsArray;

        // Group skills by category
        var skillsByCategory = new ScriptObject();
        foreach (var group in skills.GroupBy(s => s.Category))
        {
            skillsByCategory[group.Key] = new ScriptArray(group.Select(s => s.Name).ToList());
        }
        portfolioObj["skills_by_category"] = skillsByCategory;

        model["portfolio"] = portfolioObj;
        model["css"] = css;
        model["year"] = DateTime.UtcNow.Year;
        
        return model;
    }

    private static ScriptArray ParseHighlights(string? aiHighlights)
    {
        if (string.IsNullOrEmpty(aiHighlights))
        {
            return new ScriptArray();
        }

        try
        {
            var highlights = System.Text.Json.JsonSerializer.Deserialize<List<string>>(aiHighlights);
            return new ScriptArray(highlights ?? []);
        }
        catch
        {
            return new ScriptArray();
        }
    }
}

using Microsoft.Extensions.Logging;
using Preznt.Core.Common;
using Preznt.Core.Entities;
using Preznt.Core.Interfaces.Repositories;
using Preznt.Core.Interfaces.Services;

namespace Preznt.Infrastructure.Services;

public sealed class PortfolioService : IPortfolioService
{
    private const int MaxPortfoliosPerUser = 3;
    private const int MaxRepositoriesPerPortfolio = 4;
    private readonly IPortfolioRepository _portfolioRepository;
    private readonly ILogger<PortfolioService> _logger;

    public PortfolioService(
        IPortfolioRepository portfolioRepository,
        ILogger<PortfolioService> logger)
    {
        _portfolioRepository = portfolioRepository;
        _logger = logger;
    }

    public async Task<Result<PortfolioResponse>> CreateAsync(
        Guid userId,
        CreatePortfolioRequest request,
        CancellationToken ct = default)
    {
        var count = await _portfolioRepository.GetCountByUserIdAsync(userId, ct);
        if (count >= MaxPortfoliosPerUser)
        {
            return Result<PortfolioResponse>.Failure(
                new ResultError(ErrorType.Forbidden, $"User has reached the maximum number of portfolios ({MaxPortfoliosPerUser})"));
        }

        var slug = await GenerateUniqueSlugAsync(userId, request.DisplayName, ct);

        var portfolio = Portfolio.Create(
            userId: userId,
            slug: slug,
            displayName: request.DisplayName,
            position: request.Position,
            company: request.Company,
            bio: request.Bio,
            location: request.Location,
            profileImageUrl: request.ProfileImageUrl,
            gitHubUrl: request.GitHubUrl,
            themeId: request.ThemeId);
            
        _portfolioRepository.Add(portfolio);
        await _portfolioRepository.SaveChangesAsync(ct);

        _logger.LogInformation("Portfolio {PortfolioId} created for User {UserId}", portfolio.Id, userId);

        return Result<PortfolioResponse>.Success(ToResponse(portfolio));
    }

    public async Task<Result<PortfolioResponse>> GetByIdAsync(Guid userId, Guid portfolioId, CancellationToken ct = default)
    {
        var portfolio = await _portfolioRepository.GetByIdWithDetailsAsync(portfolioId, ct);

        if (portfolio is null || portfolio.UserId != userId)
        {
            return Result<PortfolioResponse>.NotFound("Portfolio", portfolioId.ToString());
        }

        return Result<PortfolioResponse>.Success(ToResponse(portfolio));
    }

    public async Task<Result<IReadOnlyList<PortfolioListItem>>> GetAllAsync(Guid userId, CancellationToken ct = default)
    {
        var portfolios = await _portfolioRepository.GetByUserIdAsync(userId, ct);
        
        var items = portfolios
            .Select(p => new PortfolioListItem(
                p.Id,
                p.Slug,
                p.Status.ToString(),
                p.DisplayName,
                p.PublishedUrl,
                p.Projects.Count,
                p.Skills.Count,
                p.UpdatedAt))
            .ToList();

        return Result<IReadOnlyList<PortfolioListItem>>.Success(items);
    }

    public async Task<Result<PortfolioResponse>> UpdateAsync(Guid userId, Guid portfolioId, UpdatePortfolioRequest request, CancellationToken ct = default)
    {
        var portfolio = await _portfolioRepository.GetByIdWithDetailsAsync(portfolioId, ct);

        if (portfolio is null || portfolio.UserId != userId)
        {
            return Result<PortfolioResponse>.NotFound("Portfolio", portfolioId.ToString());
        }

        // Regenerate slug if display name has changed
        if (!string.IsNullOrEmpty(request.DisplayName) && request.DisplayName != portfolio.DisplayName)
        {
            var newSlug = await GenerateUniqueSlugAsync(userId, request.DisplayName, ct);
            portfolio.UpdateSlug(newSlug);
        }

        portfolio.UpdatePersonalInfo(
            displayName: request.DisplayName ?? portfolio.DisplayName,
            position: request.Position ?? portfolio.Position,
            company: request.Company ?? portfolio.Company,
            bio: request.Bio ?? portfolio.Bio,
            location: request.Location ?? portfolio.Location,
            contactEmail: request.ContactEmail ?? portfolio.ContactEmail);

        portfolio.UpdateSocialLinks(
            gitHubUrl: request.GitHubUrl ?? portfolio.GitHubUrl,
            linkedInUrl: request.LinkedInUrl ?? portfolio.LinkedInUrl,
            twitterUrl: request.TwitterUrl ?? portfolio.TwitterUrl,
            websiteUrl: request.WebsiteUrl ?? portfolio.WebsiteUrl);

        if (request.ProfileImageUrl is not null)
        {
            portfolio.SetProfileImage(request.ProfileImageUrl);
        }

        if (!string.IsNullOrEmpty(request.ThemeId))
        {
            portfolio.SetTheme(request.ThemeId);
        }

        _portfolioRepository.Update(portfolio);
        await _portfolioRepository.SaveChangesAsync(ct);

        return Result<PortfolioResponse>.Success(ToResponse(portfolio));
    }

    public async Task<Result<bool>> DeleteAsync(Guid userId, Guid portfolioId, CancellationToken ct = default)
    {
        var portfolio = await _portfolioRepository.GetByIdWithDetailsAsync(portfolioId, ct);

        if (portfolio is null || portfolio.UserId != userId)
        {
            return Result<bool>.NotFound("Portfolio", portfolioId.ToString());
        }

        _portfolioRepository.Delete(portfolio);
        await _portfolioRepository.SaveChangesAsync(ct);

        _logger.LogInformation("Portfolio {PortfolioId} deleted for User {UserId}", portfolio.Id, userId);

        return Result<bool>.Success(true);
    }

    public async Task<Result<PortfolioResponse>> SetProjectsAsync(Guid userId, Guid portfolioId, SetProjectsRequest request, CancellationToken ct = default)
    {
        if (request.Projects.Count > MaxRepositoriesPerPortfolio)
        {
            return Result<PortfolioResponse>.Failure(
                new ResultError(ErrorType.Validation, $"Cannot generate more than {MaxRepositoriesPerPortfolio} repositories to a portfolio"));
        }

        var portfolio = await _portfolioRepository.GetByIdWithDetailsAsync(portfolioId, ct);

        if (portfolio is null || portfolio.UserId != userId)
        {
            return Result<PortfolioResponse>.NotFound("Portfolio", portfolioId.ToString());
        }

        portfolio.ClearProjects();

        for (var i = 0; i < request.Projects.Count; i++)
        {
            var input = request.Projects[i];
            var project = PortfolioProject.Create(
                portfolioId: portfolio.Id,
                gitHubRepoId: input.GitHubRepoId,
                name: input.Name,
                originalDescription: input.Description,
                repoUrl: input.RepoUrl,
                language: input.Language,
                stars: input.Stars,
                forks: input.Forks,
                displayOrder: i);

            if (input.IsFeatured)
            {
                project.SetFeatured(true);
            }

            portfolio.AddProject(project);
        }
        _portfolioRepository.Update(portfolio);
        await _portfolioRepository.SaveChangesAsync(ct);

        return Result<PortfolioResponse>.Success(ToResponse(portfolio));
    }

    public async Task<Result<PortfolioResponse>> SetSkillsAsync(
        Guid userId,
        Guid portfolioId,
        SetSkillsRequest request,
        CancellationToken ct = default)
    {
        var portfolio = await _portfolioRepository.GetByIdWithDetailsAsync(portfolioId, ct);

        if (portfolio is null || portfolio.UserId != userId)
        {
            return Result<PortfolioResponse>.NotFound("Portfolio", portfolioId.ToString());
        }

        portfolio.ClearSkills();

        for (var i = 0; i < request.Skills.Count; i++)
        {
            var input = request.Skills[i];
            var skill = PortfolioSkill.Create(
                portfolioId: portfolio.Id,
                name: input.Name,
                category: input.Category,
                displayOrder: i);

            portfolio.AddSkill(skill);
        }

        _portfolioRepository.Update(portfolio);
        await _portfolioRepository.SaveChangesAsync(ct);

        return Result<PortfolioResponse>.Success(ToResponse(portfolio));
    }


    private async Task<string> GenerateUniqueSlugAsync(Guid userId, string displayName, CancellationToken ct)
    {
        var baseSlug = SlugHelper.GenerateSlug(displayName);
        var slug = baseSlug;
        var suffix = 1;

        while (await _portfolioRepository.SlugExistsAsync(userId, slug, null, ct))
        {
            slug = $"{baseSlug}-{suffix}";
            suffix++;
        }

        return slug;
    }

    private static PortfolioResponse ToResponse(Portfolio portfolio)
    {
        return new PortfolioResponse(
            Id: portfolio.Id,
            Slug: portfolio.Slug,
            Status: portfolio.Status.ToString(),
            DisplayName: portfolio.DisplayName,
            Position: portfolio.Position,
            Company: portfolio.Company,
            Bio: portfolio.Bio,
            Location: portfolio.Location,
            ContactEmail: portfolio.ContactEmail,
            ProfileImageUrl: portfolio.ProfileImageUrl,
            GitHubUrl: portfolio.GitHubUrl,
            LinkedInUrl: portfolio.LinkedInUrl,
            TwitterUrl: portfolio.TwitterUrl,
            WebsiteUrl: portfolio.WebsiteUrl,
            ThemeId: portfolio.ThemeId,
            PublishedUrl: portfolio.PublishedUrl,
            PublishedAt: portfolio.PublishedAt,
            CreatedAt: portfolio.CreatedAt,
            UpdatedAt: portfolio.UpdatedAt,
            Projects: portfolio.Projects.Select(ToProjectResponse).ToList(),
            Skills: portfolio.Skills.Select(ToSkillResponse).ToList());
    }

    private static ProjectResponse ToProjectResponse(PortfolioProject project)
    {
        return new ProjectResponse(
            Id: project.Id,
            GitHubRepoId: project.GitHubRepoId,
            Name: project.Name,
            OriginalDescription: project.OriginalDescription,
            AiDescription: project.AiDescription,
            AiHighlights: project.AiHighlights,
            RepoUrl: project.RepoUrl,
            Language: project.Language,
            Stars: project.Stars,
            Forks: project.Forks,
            DisplayOrder: project.DisplayOrder,
            IsFeatured: project.IsFeatured);
    }

    private static SkillResponse ToSkillResponse(PortfolioSkill skill)
    {
        return new SkillResponse(
            Id: skill.Id,
            Name: skill.Name,
            Category: skill.Category.ToString(),
            DisplayOrder: skill.DisplayOrder);
    }
}
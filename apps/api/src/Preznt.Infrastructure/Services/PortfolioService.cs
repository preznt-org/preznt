using Microsoft.Extensions.Logging;
using Preznt.Core.Common;
using Preznt.Core.Entities;
using Preznt.Core.Interfaces.Repositories;
using Preznt.Core.Interfaces.Services;

namespace Preznt.Core.Services;

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
        var count = _portfolioRepository.GetCountByUserIdAsync(userId, ct);
        if (count.Result >= MaxPortfoliosPerUser)
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

    public Task<Result<bool>> DeleteAsync(Guid userId, Guid portfolioId, CancellationToken ct = default)
    {
        throw new NotImplementedException();
    }

    public Task<Result<IReadOnlyList<PortfolioListItem>>> GetAllAsync(Guid userId, CancellationToken ct = default)
    {
        throw new NotImplementedException();
    }

    public Task<Result<PortfolioResponse>> GetByIdAsync(Guid userId, Guid portfolioId, CancellationToken ct = default)
    {
        throw new NotImplementedException();
    }

    public Task<Result<PortfolioResponse>> SetProjectsAsync(Guid userId, Guid portfolioId, SetProjectsRequest request, CancellationToken ct = default)
    {
        throw new NotImplementedException();
    }

    public Task<Result<PortfolioResponse>> SetSkillsAsync(Guid userId, Guid portfolioId, SetSkillsRequest request, CancellationToken ct = default)
    {
        throw new NotImplementedException();
    }

    public Task<Result<PortfolioResponse>> UpdateAsync(Guid userId, Guid portfolioId, UpdatePortfolioRequest request, CancellationToken ct = default)
    {
        throw new NotImplementedException();
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
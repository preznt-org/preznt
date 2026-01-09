using Preznt.Core.Common;
using Preznt.Core.Entities;

namespace Preznt.Core.Interfaces.Services;

public interface IPortfolioService
{
    Task<Result<PortfolioResponse>> CreateAsync(Guid userId, CreatePortfolioRequest request, CancellationToken ct = default);
    Task<Result<PortfolioResponse>> GetByIdAsync(Guid userId, Guid portfolioId, CancellationToken ct = default);
    Task<Result<IReadOnlyList<PortfolioListItem>>> GetAllAsync(Guid userId, CancellationToken ct = default);
    Task<Result<PortfolioResponse>> UpdateAsync(Guid userId, Guid portfolioId, UpdatePortfolioRequest request, CancellationToken ct = default);
    Task<Result<bool>> DeleteAsync(Guid userId, Guid portfolioId, CancellationToken ct = default);
    Task<Result<PortfolioResponse>> SetProjectsAsync(Guid userId, Guid portfolioId, SetProjectsRequest request, CancellationToken ct = default);
    Task<Result<PortfolioResponse>> SetSkillsAsync(Guid userId, Guid portfolioId, SetSkillsRequest request, CancellationToken ct = default);
}

// Request DTOs
public sealed record CreatePortfolioRequest(
    string DisplayName,
    string? Position = null,
    string? Company = null,
    string? Bio = null,
    string? Location = null,
    string? ProfileImageUrl = null,
    string? GitHubUrl = null,
    string ThemeId = "default");

public sealed record UpdatePortfolioRequest(
    string? DisplayName = null,
    string? Position = null,
    string? Company = null,
    string? Bio = null,
    string? Location = null,
    string? ContactEmail = null,
    string? ProfileImageUrl = null,
    string? GitHubUrl = null,
    string? LinkedInUrl = null,
    string? TwitterUrl = null,
    string? WebsiteUrl = null,
    string? ThemeId = null);

public sealed record SetProjectsRequest(IReadOnlyList<ProjectInput> Projects);

public sealed record ProjectInput(
    long GitHubRepoId,
    string Name,
    string? Description,
    string RepoUrl,
    string? Language,
    int Stars,
    int Forks,
    bool IsFeatured = false);

public sealed record SetSkillsRequest(IReadOnlyList<SkillInput> Skills);

public sealed record SkillInput(
    string Name,
    SkillCategory Category);

// Response DTOs
public sealed record PortfolioResponse(
    Guid Id,
    string Slug,
    string Status,
    string DisplayName,
    string? Position,
    string? Company,
    string? Bio,
    string? Location,
    string? ContactEmail,
    string? ProfileImageUrl,
    string? GitHubUrl,
    string? LinkedInUrl,
    string? TwitterUrl,
    string? WebsiteUrl,
    string ThemeId,
    string? PublishedUrl,
    DateTime? PublishedAt,
    DateTime CreatedAt,
    DateTime UpdatedAt,
    IReadOnlyList<ProjectResponse> Projects,
    IReadOnlyList<SkillResponse> Skills);

public sealed record PortfolioListItem(
    Guid Id,
    string Slug,
    string Status,
    string DisplayName,
    string? PublishedUrl,
    int ProjectCount,
    int SkillCount,
    DateTime UpdatedAt);

public sealed record ProjectResponse(
    Guid Id,
    long GitHubRepoId,
    string Name,
    string? OriginalDescription,
    string? AiDescription,
    string? AiHighlights,
    string RepoUrl,
    string? Language,
    int Stars,
    int Forks,
    int DisplayOrder,
    bool IsFeatured);

public sealed record SkillResponse(
    Guid Id,
    string Name,
    string Category,
    int DisplayOrder);
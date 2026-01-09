namespace Preznt.Core.Entities;

public sealed class PortfolioProject
{
    public Guid Id { get; private set; }
    public Guid PortfolioId { get; private set; }
    
    // GitHub repo info
    public long GitHubRepoId { get; private set; }
    public string Name { get; private set; } = null!;
    public string? OriginalDescription { get; private set; }
    public string RepoUrl { get; private set; } = null!;
    public string? Language { get; private set; }
    public int Stars { get; private set; }
    public int Forks { get; private set; }
    
    // AI-enhanced content (from deepwiki)
    public string? AiDescription { get; private set; }
    public string? AiHighlights { get; private set; } // JSON array of key features
    
    // Display
    public int DisplayOrder { get; private set; }
    public bool IsFeatured { get; private set; }
    
    // Timestamps
    public DateTime CreatedAt { get; private set; }
    public DateTime UpdatedAt { get; private set; }

    // Navigation
    public Portfolio Portfolio { get; private set; } = null!;

    private PortfolioProject() { }

    public static PortfolioProject Create(
        Guid portfolioId,
        long gitHubRepoId,
        string name,
        string? originalDescription,
        string repoUrl,
        string? language,
        int stars,
        int forks,
        int displayOrder)
    {
        return new PortfolioProject
        {
            Id = Guid.NewGuid(),
            PortfolioId = portfolioId,
            GitHubRepoId = gitHubRepoId,
            Name = name,
            OriginalDescription = originalDescription,
            RepoUrl = repoUrl,
            Language = language,
            Stars = stars,
            Forks = forks,
            DisplayOrder = displayOrder,
            IsFeatured = false,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
    }

    public void SetAiContent(string? aiDescription, string? aiHighlights)
    {
        AiDescription = aiDescription;
        AiHighlights = aiHighlights;
        UpdatedAt = DateTime.UtcNow;
    }

    public void UpdateDisplayOrder(int order)
    {
        DisplayOrder = order;
        UpdatedAt = DateTime.UtcNow;
    }

    public void SetFeatured(bool isFeatured)
    {
        IsFeatured = isFeatured;
        UpdatedAt = DateTime.UtcNow;
    }
}

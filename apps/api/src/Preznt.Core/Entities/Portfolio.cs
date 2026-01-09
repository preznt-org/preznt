namespace Preznt.Core.Entities;

public sealed class Portfolio
{
    public Guid Id { get; private set; }
    public Guid UserId { get; private set; }
    
    // Basic
    public string Slug { get; private set; } = null!;
    public PortfolioStatus Status { get; private set; }
    
    // Personal Info
    public string DisplayName { get; private set; } = null!;
    public string? Position { get; private set; }
    public string? Company { get; private set; }
    public string? Bio { get; private set; }
    public string? Location { get; private set; }
    public string? ProfileImageUrl { get; private set; }
    public string? ContactEmail { get; private set; }
    
    // Social Links
    public string? GitHubUrl { get; private set; }
    public string? LinkedInUrl { get; private set; }
    public string? TwitterUrl { get; private set; }
    public string? WebsiteUrl { get; private set; }
    
    // Theme & Customization
    public string ThemeId { get; private set; } = "default";
    public string? CustomCss { get; private set; }
    
    // Publishing
    public string? PublishedUrl { get; private set; }
    public DateTime? PublishedAt { get; private set; }
    
    // Timestamps
    public DateTime CreatedAt { get; private set; }
    public DateTime UpdatedAt { get; private set; }

    // Navigation
    public User User { get; private set; } = null!;
    private readonly List<PortfolioProject> _projects = [];
    public IReadOnlyList<PortfolioProject> Projects => _projects.AsReadOnly();
    
    private readonly List<PortfolioSkill> _skills = [];
    public IReadOnlyList<PortfolioSkill> Skills => _skills.AsReadOnly();

    private Portfolio() { }

    public static Portfolio Create(
        Guid userId,
        string slug,
        string displayName,
        string? position = null,
        string? company = null,
        string? bio = null,
        string? location = null,
        string? profileImageUrl = null,
        string? gitHubUrl = null,
        string themeId = "default")
    {
        return new Portfolio
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            Slug = slug,
            DisplayName = displayName,
            Position = position,
            Company = company,
            Bio = bio,
            Location = location,
            ProfileImageUrl = profileImageUrl,
            GitHubUrl = gitHubUrl,
            ThemeId = themeId,
            Status = PortfolioStatus.Draft,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
    }
    
    public void UpdateSlug(string slug)
    {
        Slug = slug;
        UpdatedAt = DateTime.UtcNow;
    }

    public void UpdatePersonalInfo(
        string displayName,
        string? position,
        string? company,
        string? bio,
        string? location,
        string? contactEmail)
    {
        DisplayName = displayName;
        Position = position;
        Company = company;
        Bio = bio;
        Location = location;
        ContactEmail = contactEmail;
        UpdatedAt = DateTime.UtcNow;
    }

    public void UpdateSocialLinks(
        string? gitHubUrl,
        string? linkedInUrl,
        string? twitterUrl,
        string? websiteUrl)
    {
        GitHubUrl = gitHubUrl;
        LinkedInUrl = linkedInUrl;
        TwitterUrl = twitterUrl;
        WebsiteUrl = websiteUrl;
        UpdatedAt = DateTime.UtcNow;
    }

    public void SetProfileImage(string? profileImageUrl)
    {
        ProfileImageUrl = profileImageUrl;
        UpdatedAt = DateTime.UtcNow;
    }

    public void SetTheme(string themeId, string? customCss = null)
    {
        ThemeId = themeId;
        CustomCss = customCss;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Publish(string publishedUrl)
    {
        Status = PortfolioStatus.Published;
        PublishedUrl = publishedUrl;
        PublishedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Unpublish()
    {
        Status = PortfolioStatus.Draft;
        PublishedUrl = null;
        UpdatedAt = DateTime.UtcNow;
    }

    public void AddProject(PortfolioProject project)
    {
        _projects.Add(project);
        UpdatedAt = DateTime.UtcNow;
    }

    public void RemoveProject(Guid projectId)
    {
        var project = _projects.FirstOrDefault(p => p.Id == projectId);
        if (project is not null)
        {
            _projects.Remove(project);
            UpdatedAt = DateTime.UtcNow;
        }
    }

    public void ClearProjects()
    {
        _projects.Clear();
        UpdatedAt = DateTime.UtcNow;
    }

    public void AddSkill(PortfolioSkill skill)
    {
        _skills.Add(skill);
        UpdatedAt = DateTime.UtcNow;
    }

    public void ClearSkills()
    {
        _skills.Clear();
        UpdatedAt = DateTime.UtcNow;
    }
}

public enum PortfolioStatus
{
    Draft = 0,
    Published = 1
}

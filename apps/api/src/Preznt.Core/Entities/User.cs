namespace Preznt.Core.Entities;

public sealed class User
{
    public Guid Id {get; private set; }
    public long GitHubId { get; private set; }
    public string Username {get; private set; } = null!;
    public string? Email {get; private set; }
    public string? Name {get; private set; }
    public string? AvatarUrl {get; private set; }
    public string GitHubAccessToken { get; private set; } = null!;
    public string? RefreshTokenHash { get; private set; }
    public DateTime? RefreshTokenExpiresAt { get; private set; }
    public DateTime? TokensInvalidatedAt { get; private set; }
    public DateTime CreatedAt {get; private set; }
    public DateTime UpdatedAt {get; private set; }

    // For EF Core
    private User() { }

    public static User Create(
        long gitHubId,
        string username,
        string? email,
        string? name,
        string? avatarUrl,
        string gitHubAccessToken)
    {
        return new User
        {
            Id = Guid.NewGuid(),
            GitHubId = gitHubId,
            Username = username,
            Email = email,
            Name = name,
            AvatarUrl = avatarUrl,
            GitHubAccessToken = gitHubAccessToken,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
    }

    public void UpdateFromGitHub(
        string username,
        string? email,
        string? name,
        string? avatarUrl,
        string gitHubAccessToken)
    {
        Username = username;
        Email = email;
        Name = name;
        AvatarUrl = avatarUrl;
        GitHubAccessToken = gitHubAccessToken;
        UpdatedAt = DateTime.UtcNow;
    }

    public void SetRefreshToken(string tokenHash, DateTime expiresAt)
    {
        RefreshTokenHash = tokenHash;
        RefreshTokenExpiresAt = expiresAt;
        UpdatedAt = DateTime.UtcNow;
    }

    public void ClearRefreshToken()
    {
        RefreshTokenHash = null;
        RefreshTokenExpiresAt = null;
        TokensInvalidatedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }

    public bool ValidateRefreshToken(string tokenHash)
    {
        return RefreshTokenHash == tokenHash 
            && RefreshTokenExpiresAt > DateTime.UtcNow;
    }
}
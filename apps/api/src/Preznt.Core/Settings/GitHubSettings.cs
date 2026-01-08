using Preznt.Core.Constants;

namespace Preznt.Core.Settings;

public sealed class GitHubSettings
{
    public const string SectionName = "GitHub";

    public required string ClientId { get; init; }
    public required string ClientSecret { get; init; }
    public required string CallbackUrl { get; init; }
    public string[] Scopes { get; init;} = ["user:email", "read:user", "repo", "delete_repo"];
}

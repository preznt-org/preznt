namespace Preznt.Core.Common;

public sealed record Theme(
    string Id,
    string Name,
    string Description,
    string PreviewImageUrl);

public sealed record SamplePortfolio(
    string DisplayName,
    string Position,
    string Company,
    string Location,
    string Bio,
    string AvatarUrl,
    string GitHubUrl,
    string LinkedInUrl,
    IReadOnlyList<SampleProject> Projects,
    IReadOnlyList<SampleSkill> Skills);

public sealed record SampleProject(
    string Name,
    string Description,
    string Language,
    int Stars,
    int Forks,
    string RepoUrl);

public sealed record SampleSkill(
    string Name,
    string Category);

public static class AvailableThemes
{
    public static readonly IReadOnlyList<Theme> All =
    [
        new Theme(
            Id: "minimal-light",
            Name: "Minimal Light",
            Description: "Clean, professional light theme with subtle accents",
            PreviewImageUrl: "/themes/minimal-light.png"),

        new Theme(
            Id: "minimal-dark",
            Name: "Minimal Dark",
            Description: "Sleek dark theme for a modern look",
            PreviewImageUrl: "/themes/minimal-dark.png"),

        new Theme(
            Id: "modern",
            Name: "Modern",
            Description: "Bold, contemporary design with vibrant colors",
            PreviewImageUrl: "/themes/modern.png"),

        new Theme(
            Id: "modern-dark",
            Name: "Modern Dark",
            Description: "Bold dark theme with vibrant gradients",
            PreviewImageUrl: "/themes/modern-dark.png"),

        new Theme(
            Id: "developer",
            Name: "Developer",
            Description: "Terminal-inspired theme for the tech-savvy",
            PreviewImageUrl: "/themes/developer.png")
    ];

    public static Theme? GetById(string id) => 
        All.FirstOrDefault(t => t.Id == id);

    public static bool Exists(string id) => 
        All.Any(t => t.Id == id);

    public const string DefaultThemeId = "minimal-light";

    public static readonly SamplePortfolio SampleData = new(
        DisplayName: "Alex Johnson",
        Position: "Full Stack Developer",
        Company: "TechCorp",
        Location: "San Francisco, CA",
        Bio: "Passionate developer with 5+ years of experience building scalable web applications. I love clean code, open source, and turning complex problems into simple solutions.",
        AvatarUrl: "https://avatars.githubusercontent.com/u/141199348?s=400&u=8e8824154a6dc4ac0e64c027c8a343a28a55e4e8&v=4",
        GitHubUrl: "https://github.com/m7mdraafat",
        LinkedInUrl: "https://linkedin.com/in/m7mdraafat",
        Projects:
        [
            new SampleProject(
                Name: "TaskFlow API",
                Description: "A robust REST API for task management built with .NET 8 and PostgreSQL. Features real-time updates via SignalR, JWT authentication, and comprehensive API documentation.",
                Language: "C#",
                Stars: 128,
                Forks: 34,
                RepoUrl: "https://github.com/m7mdraafat"),
            new SampleProject(
                Name: "React Dashboard",
                Description: "Modern admin dashboard with dark mode, real-time charts, and responsive design. Built with React, TypeScript, and Tailwind CSS.",
                Language: "TypeScript",
                Stars: 89,
                Forks: 21,
                RepoUrl: "https://github.com/m7mdraafat"),
            new SampleProject(
                Name: "DevCLI",
                Description: "Command-line toolkit for developers. Automates common tasks like project scaffolding, git workflows, and deployment scripts.",
                Language: "Go",
                Stars: 256,
                Forks: 45,
                RepoUrl: "https://github.com/m7mdraafat")
        ],
        Skills:
        [
            new SampleSkill("C#", "Language"),
            new SampleSkill("TypeScript", "Language"),
            new SampleSkill("Go", "Language"),
            new SampleSkill("React", "Framework"),
            new SampleSkill(".NET", "Framework"),
            new SampleSkill("PostgreSQL", "Database"),
            new SampleSkill("Redis", "Database"),
            new SampleSkill("Docker", "Tool"),
            new SampleSkill("Azure", "Cloud")
        ]);
}
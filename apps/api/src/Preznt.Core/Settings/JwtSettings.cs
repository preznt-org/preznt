namespace Preznt.Core.Settings;

public sealed class JwtSettings
{
    public const string SectionName = "Jwt";

    public required string SecretKey { get; init; }
    public required string Issuer { get; init; }
    public required string Audience { get; init; }
    public required int ExpiryInMinutes { get; init; } = 30;
}
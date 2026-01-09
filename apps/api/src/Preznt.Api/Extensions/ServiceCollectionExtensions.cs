namespace Preznt.Api.Extensions;

using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Preznt.Core.Interfaces.Repositories;
using Preznt.Core.Interfaces.Services;
using Preznt.Core.Settings;
using Preznt.Infrastructure.Data;
using Preznt.Infrastructure.Repositories;
using Preznt.Infrastructure.Services;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddPrezntServices(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // Settings
        services.Configure<GitHubSettings>(configuration.GetSection(GitHubSettings.SectionName));
        services.Configure<JwtSettings>(configuration.GetSection(JwtSettings.SectionName));

        // Database
        services.AddDbContext<PrezntDbContext>(options =>
            options.UseNpgsql(configuration.GetConnectionString("Database")));

        // Repositories
        services.AddScoped<IUserRepository, UserRepository>();

        // Services
        services.AddHttpClient<IGitHubAuthService, GitHubAuthService>();
        services.AddScoped<IJwtService, JwtService>();
        services.AddScoped<IAuthService, AuthService>();

        return services;
    }

    public static IServiceCollection AddPrezntAuthentication(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var jwtSettings = configuration.GetSection(JwtSettings.SectionName).Get<JwtSettings>()
            ?? throw new InvalidOperationException("JWT settings not configured");

        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = jwtSettings.Issuer,
                    ValidAudience = jwtSettings.Audience,
                    IssuerSigningKey = new SymmetricSecurityKey(
                        Encoding.UTF8.GetBytes(jwtSettings.SecretKey)),
                    ClockSkew = TimeSpan.Zero
                };
            });

        services.AddAuthorization();

        return services;
    }
}
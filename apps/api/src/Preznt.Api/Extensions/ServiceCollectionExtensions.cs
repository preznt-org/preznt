namespace Preznt.Api.Extensions;

using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
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
        services.AddScoped<IPortfolioRepository, PortfolioRepository>();
        
        // Services
        services.AddSingleton<IGitHubService, GitHubService>();
        services.AddScoped<IJwtService, JwtService>();
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IPortfolioService, PortfolioService>();

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

                options.Events = new JwtBearerEvents
                {
                    OnTokenValidated = async context =>
                    {
                        var userRepo = context.HttpContext.RequestServices.GetRequiredService<IUserRepository>();
                        
                        var userIdClaim = context.Principal?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                        if (!Guid.TryParse(userIdClaim, out var userId))
                        {
                            context.Fail("Invalid user ID");
                            return;
                        }

                        var user = await userRepo.GetByIdAsync(userId);
                        if (user is null)
                        {
                            context.Fail("User not found");
                            return;
                        }

                        // Check if token was issued before logout
                        if (user.TokensInvalidatedAt.HasValue)
                        {
                            var iatClaim = context.Principal?.FindFirst(JwtRegisteredClaimNames.Iat)?.Value;
                            if (!long.TryParse(iatClaim, out var iatUnix))
                            {
                                // No iat claim means old token - reject it
                                context.Fail("Token missing issued-at claim");
                                return;
                            }
                            
                            var issuedAt = DateTimeOffset.FromUnixTimeSeconds(iatUnix).UtcDateTime;
                            if (issuedAt < user.TokensInvalidatedAt.Value)
                            {
                                context.Fail("Token has been invalidated");
                                return;
                            }
                        }
                    }
                };
            });

        services.AddAuthorization();

        return services;
    }
}
using Preznt.Api.Endpoints;
using Preznt.Api.Extensions;

// Load .env file from solution root
EnvironmentExtensions.LoadDotEnv();

var builder = WebApplication.CreateBuilder(args);

// Add environment variables to configuration (picks up values from .env)
builder.Configuration.AddEnvironmentVariables();

builder.Services.AddPrezntServices(builder.Configuration);
builder.Services.AddPrezntAuthentication(builder.Configuration);

// Add ProblemDetails for consistent error responses
builder.Services.AddProblemDetails();

// Add CORS for frontend
builder.Services.AddCors(options =>
{
    options.AddPolicy("Frontend", policy =>
    {
        var frontendUrl = builder.Configuration["Frontend:BaseUrl"] ?? "http://localhost:5173";
        policy.WithOrigins(frontendUrl)
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
});

var app = builder.Build();

app.UseHttpsRedirection();
app.UseCors("Frontend");

app.UseAuthentication();
app.UseAuthorization();

var api = app.MapGroup("/api");

api.MapGroup("/auth")
    .MapAuthEndpoints()
    .WithTags("Authentication");

// Health check endpoints
app.MapGet("/health", () => Results.Ok(new { status = "healthy", timestamp = DateTime.UtcNow }))
   .WithTags("Health");

app.Run();
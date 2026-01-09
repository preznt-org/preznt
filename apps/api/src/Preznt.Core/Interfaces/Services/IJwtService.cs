namespace Preznt.Core.Interfaces.Services;

using Preznt.Core.Entities;

public interface IJwtService
{
    string GenerateAccessToken(User user);
    (string Token, string Hash, DateTime ExpiresAt) GenerateRefreshToken();
    DateTime GetAccessTokenExpiry();
}
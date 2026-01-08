namespace Preznt.Core.Interfaces.Services;

using Preznt.Core.Entities;

public interface IJwtService
{
    string GenerateToken(User user);
    DateTime GetExpiryDate();
}
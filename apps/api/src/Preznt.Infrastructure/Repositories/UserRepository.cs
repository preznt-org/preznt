using Microsoft.EntityFrameworkCore;
using Preznt.Core.Entities;
using Preznt.Core.Interfaces.Repositories;
using Preznt.Infrastructure.Data;

namespace Preznt.Infrastructure.Repositories;

public sealed class UserRepository : IUserRepository
{
    private readonly PrezntDbContext _dbContext;
    
    public UserRepository(PrezntDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<User?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        return await _dbContext.Users.FindAsync([id], ct);
    }

    public async Task<User?> GetByGitHubIdAsync(long gitHubId, CancellationToken ct = default)
    {
        return await _dbContext.Users
            .FirstOrDefaultAsync(u => u.GitHubId == gitHubId, ct);
    }

    public async Task<User?> GetByUsernameAsync(string username, CancellationToken ct = default)
    {
        return await _dbContext.Users
            .FirstOrDefaultAsync(u => u.Username == username, ct);
    }

    public async Task<User?> GetByRefreshTokenHashAsync(string tokenHash, CancellationToken ct = default)
    {
        return await _dbContext.Users
            .FirstOrDefaultAsync(u => u.RefreshTokenHash == tokenHash, ct);
    }

    public void Add(User user) => _dbContext.Users.Add(user);
    
    public void Update(User user) => _dbContext.Users.Update(user);
    
    public async Task SaveChangesAsync(CancellationToken ct = default)
    {
        await _dbContext.SaveChangesAsync(ct);
    }
}
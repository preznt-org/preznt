using Preznt.Core.Entities;

namespace Preznt.Core.Interfaces.Repositories;

public interface IUserRepository
{
    Task<User?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<User?> GetByGitHubIdAsync(long gitHubId, CancellationToken ct = default);
    Task<User?> GetByRefreshTokenHashAsync(string tokenHash, CancellationToken ct = default);
    void Add(User user);
    void Update(User user);
    Task SaveChangesAsync(CancellationToken ct = default);
}
using Preznt.Core.Entities;

namespace Preznt.Core.Interfaces.Repositories;

public interface IUserRepository
{
    Task<User?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<User?> GetByGitHubIdAsync(long gitHubId, CancellationToken cancellationToken = default);
    void Add(User user);
    void Update(User user);
    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}
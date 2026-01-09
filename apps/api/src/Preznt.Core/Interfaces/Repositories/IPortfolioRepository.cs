using Preznt.Core.Entities;

namespace Preznt.Core.Interfaces.Repositories;

public interface IPortfolioRepository
{
    // Queries
    Task<Portfolio?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<Portfolio?> GetByIdWithDetailsAsync(Guid id, CancellationToken ct = default);
    Task<IReadOnlyList<Portfolio>> GetByUserIdAsync(Guid userId, CancellationToken ct = default);
    Task<int> GetCountByUserIdAsync(Guid userId, CancellationToken ct = default);
    Task<bool> SlugExistsAsync(Guid userId, string slug, Guid? excludeId = null, CancellationToken ct = default);
    
    // Commands
    void Add(Portfolio portfolio);
    void Update(Portfolio portfolio);
    void Delete(Portfolio portfolio);
    
    // Persistence
    Task SaveChangesAsync(CancellationToken ct = default);
}
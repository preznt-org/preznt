using Microsoft.EntityFrameworkCore;
using Preznt.Core.Entities;
using Preznt.Core.Interfaces.Repositories;
using Preznt.Infrastructure.Data;

namespace Preznt.Infrastructure.Repositories;

public sealed class PortfolioRepository : IPortfolioRepository
{
    private readonly PrezntDbContext _dbContext;
    
    public PortfolioRepository(PrezntDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Portfolio?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        return await _dbContext.Portfolios.FindAsync([id], ct);
    }

    public async Task<Portfolio?> GetByIdWithDetailsAsync(Guid id, CancellationToken ct = default)
    {
        var portfolio = await _dbContext.Portfolios
            .Include(p => p.Projects.OrderBy(pr => pr.DisplayOrder))
            .Include(p => p.Skills.OrderBy(s => s.DisplayOrder))
            .FirstOrDefaultAsync(p => p.Id == id, ct);

        return portfolio;
    }

    public async Task<IReadOnlyList<Portfolio>> GetByUserIdAsync(Guid userId, CancellationToken ct = default)
    {
        return await _dbContext.Portfolios
            .Where(p => p.UserId == userId)
            .Include(p => p.Projects)
            .Include(p => p.Skills)
            .OrderByDescending(p => p.UpdatedAt)
            .ToListAsync(ct);
    }

    public async Task<int> GetCountByUserIdAsync(Guid userId, CancellationToken ct = default)
    {
        return await _dbContext.Portfolios
            .CountAsync(p => p.UserId == userId, ct);
    }

    public async Task<bool> SlugExistsAsync(Guid userId, string slug, Guid? excludeId = null, CancellationToken ct = default)
    {
        var query = _dbContext.Portfolios
            .Where(p => p.UserId == userId && p.Slug == slug);

        if (excludeId.HasValue)
        {
            query = query.Where(p => p.Id != excludeId.Value);
        }

        return await query.AnyAsync(ct);
    }

    public void Add(Portfolio portfolio)
    {
        _dbContext.Portfolios.Add(portfolio);
    }

    public void Delete(Portfolio portfolio)
    {
        _dbContext.Portfolios.Remove(portfolio);
    }

    public void Update(Portfolio portfolio)
    {
        _dbContext.Portfolios.Update(portfolio);
    }

    public async Task SaveChangesAsync(CancellationToken ct = default)
    {
        await _dbContext.SaveChangesAsync(ct);
    }
}
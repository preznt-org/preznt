using Microsoft.EntityFrameworkCore;
using Preznt.Core.Entities;

namespace Preznt.Infrastructure.Data;

public sealed class PrezntDbContext : DbContext
{
    public PrezntDbContext(DbContextOptions<PrezntDbContext> options)
        : base(options)
    {
    }

    public DbSet<User> Users => Set<User>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(PrezntDbContext).Assembly);
        base.OnModelCreating(modelBuilder);
    }
}
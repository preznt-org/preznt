using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Preznt.Core.Entities;

namespace Preznt.Infrastructure.Data.Configurations;

public sealed class PortfolioProjectConfiguration : IEntityTypeConfiguration<PortfolioProject>
{
    public void Configure(EntityTypeBuilder<PortfolioProject> builder)
    {
        builder.ToTable("portfolio_projects");

        builder.HasKey(p => p.Id);

        builder.Property(p => p.Id)
            .HasColumnName("id");

        builder.Property(p => p.PortfolioId)
            .HasColumnName("portfolio_id")
            .IsRequired();

        // GitHub repo info
        builder.Property(p => p.GitHubRepoId)
            .HasColumnName("github_repo_id")
            .IsRequired();

        builder.Property(p => p.Name)
            .HasColumnName("name")
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(p => p.OriginalDescription)
            .HasColumnName("original_description")
            .HasMaxLength(1000);

        builder.Property(p => p.RepoUrl)
            .HasColumnName("repo_url")
            .HasMaxLength(500)
            .IsRequired();

        builder.Property(p => p.Language)
            .HasColumnName("language")
            .HasMaxLength(50);

        builder.Property(p => p.Stars)
            .HasColumnName("stars");

        builder.Property(p => p.Forks)
            .HasColumnName("forks");

        // AI-enhanced content
        builder.Property(p => p.AiDescription)
            .HasColumnName("ai_description")
            .HasMaxLength(2000);

        builder.Property(p => p.AiHighlights)
            .HasColumnName("ai_highlights");

        // Display
        builder.Property(p => p.DisplayOrder)
            .HasColumnName("display_order");

        builder.Property(p => p.IsFeatured)
            .HasColumnName("is_featured")
            .HasDefaultValue(false);

        // Timestamps
        builder.Property(p => p.CreatedAt)
            .HasColumnName("created_at")
            .IsRequired();

        builder.Property(p => p.UpdatedAt)
            .HasColumnName("updated_at")
            .IsRequired();

        // Indexes
        builder.HasIndex(p => p.PortfolioId);
        builder.HasIndex(p => new { p.PortfolioId, p.GitHubRepoId }).IsUnique();
    }
}

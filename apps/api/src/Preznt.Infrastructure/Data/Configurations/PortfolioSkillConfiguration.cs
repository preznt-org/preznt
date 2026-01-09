using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Preznt.Core.Entities;

namespace Preznt.Infrastructure.Data.Configurations;

public sealed class PortfolioSkillConfiguration : IEntityTypeConfiguration<PortfolioSkill>
{
    public void Configure(EntityTypeBuilder<PortfolioSkill> builder)
    {
        builder.ToTable("portfolio_skills");

        builder.HasKey(s => s.Id);

        builder.Property(s => s.Id)
            .HasColumnName("id");

        builder.Property(s => s.PortfolioId)
            .HasColumnName("portfolio_id")
            .IsRequired();

        builder.Property(s => s.Name)
            .HasColumnName("name")
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(s => s.Category)
            .HasColumnName("category")
            .IsRequired();

        builder.Property(s => s.DisplayOrder)
            .HasColumnName("display_order");

        builder.Property(s => s.CreatedAt)
            .HasColumnName("created_at")
            .IsRequired();

        // Indexes
        builder.HasIndex(s => s.PortfolioId);
        builder.HasIndex(s => new { s.PortfolioId, s.Name }).IsUnique();
    }
}

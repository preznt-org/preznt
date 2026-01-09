using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Preznt.Core.Entities;

namespace Preznt.Infrastructure.Data.Configurations;

public sealed class PortfolioConfiguration : IEntityTypeConfiguration<Portfolio>
{
    public void Configure(EntityTypeBuilder<Portfolio> builder)
    {
        builder.ToTable("portfolios");

        builder.HasKey(p => p.Id);

        builder.Property(p => p.Id)
            .HasColumnName("id");

        builder.Property(p => p.UserId)
            .HasColumnName("user_id")
            .IsRequired();

        builder.Property(p => p.Slug)
            .HasColumnName("slug")
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(p => p.Status)
            .HasColumnName("status")
            .IsRequired();

        // Personal Info
        builder.Property(p => p.DisplayName)
            .HasColumnName("display_name")
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(p => p.Position)
            .HasColumnName("position")
            .HasMaxLength(200);

        builder.Property(p => p.Company)
            .HasColumnName("company")
            .HasMaxLength(200);

        builder.Property(p => p.Bio)
            .HasColumnName("bio")
            .HasMaxLength(2000);

        builder.Property(p => p.Location)
            .HasColumnName("location")
            .HasMaxLength(200);

        builder.Property(p => p.ProfileImageUrl)
            .HasColumnName("profile_image_url")
            .HasMaxLength(500);

        builder.Property(p => p.ContactEmail)
            .HasColumnName("contact_email")
            .HasMaxLength(255);

        // Social Links
        builder.Property(p => p.GitHubUrl)
            .HasColumnName("github_url")
            .HasMaxLength(500);

        builder.Property(p => p.LinkedInUrl)
            .HasColumnName("linkedin_url")
            .HasMaxLength(500);

        builder.Property(p => p.TwitterUrl)
            .HasColumnName("twitter_url")
            .HasMaxLength(500);

        builder.Property(p => p.WebsiteUrl)
            .HasColumnName("website_url")
            .HasMaxLength(500);

        // Theme & Customization
        builder.Property(p => p.ThemeId)
            .HasColumnName("theme_id")
            .HasMaxLength(50)
            .HasDefaultValue("default");

        builder.Property(p => p.CustomCss)
            .HasColumnName("custom_css");

        // Publishing
        builder.Property(p => p.PublishedUrl)
            .HasColumnName("published_url")
            .HasMaxLength(500);

        builder.Property(p => p.PublishedAt)
            .HasColumnName("published_at");

        // Timestamps
        builder.Property(p => p.CreatedAt)
            .HasColumnName("created_at")
            .IsRequired();

        builder.Property(p => p.UpdatedAt)
            .HasColumnName("updated_at")
            .IsRequired();

        // Relationships
        builder.HasOne(p => p.User)
            .WithMany()
            .HasForeignKey(p => p.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(p => p.Projects)
            .WithOne(pr => pr.Portfolio)
            .HasForeignKey(pr => pr.PortfolioId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(p => p.Skills)
            .WithOne(s => s.Portfolio)
            .HasForeignKey(s => s.PortfolioId)
            .OnDelete(DeleteBehavior.Cascade);

        // Indexes
        builder.HasIndex(p => p.UserId);
        builder.HasIndex(p => new { p.UserId, p.Slug }).IsUnique();
    }
}

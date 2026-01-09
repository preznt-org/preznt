namespace Preznt.Core.Entities;

public sealed class PortfolioSkill
{
    public Guid Id { get; private set; }
    public Guid PortfolioId { get; private set; }
    public string Name { get; private set; } = null!;
    public SkillCategory Category { get; private set; }
    public int DisplayOrder { get; private set; }
    public DateTime CreatedAt { get; private set; }

    // Navigation
    public Portfolio Portfolio { get; private set; } = null!;

    private PortfolioSkill() { }

    public static PortfolioSkill Create(
        Guid portfolioId,
        string name,
        SkillCategory category,
        int displayOrder)
    {
        return new PortfolioSkill
        {
            Id = Guid.NewGuid(),
            PortfolioId = portfolioId,
            Name = name,
            Category = category,
            DisplayOrder = displayOrder,
            CreatedAt = DateTime.UtcNow
        };
    }

    public void UpdateDisplayOrder(int order)
    {
        DisplayOrder = order;
    }
}

public enum SkillCategory
{
    Language = 0,
    Framework = 1,
    Database = 2,
    Tool = 3,
    Cloud = 4,
    Other = 5
}

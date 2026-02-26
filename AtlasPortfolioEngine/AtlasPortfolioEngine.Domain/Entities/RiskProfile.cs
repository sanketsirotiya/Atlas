using AtlasPortfolioEngine.Domain.Enums;

namespace AtlasPortfolioEngine.Domain.Entities;

public class RiskProfile
{
    public Guid Id { get; private set; }
    public Guid ClientId { get; private set; }
    public int Score { get; private set; }
    public RiskCategory Category { get; private set; }
    public DateTime AssessedAt { get; private set; }

    private RiskProfile() { } // EF Core

    public static RiskProfile Create(Guid clientId, int score)
    {
        if (score < 0 || score > 100) throw new ArgumentOutOfRangeException(nameof(score), "Score must be between 0 and 100.");

        return new RiskProfile
        {
            Id = Guid.NewGuid(),
            ClientId = clientId,
            Score = score,
            Category = DetermineCategory(score),
            AssessedAt = DateTime.UtcNow
        };
    }

    private static RiskCategory DetermineCategory(int score) => score switch
    {
        <= 33 => RiskCategory.Conservative,
        <= 66 => RiskCategory.Balanced,
        _     => RiskCategory.Growth
    };
}
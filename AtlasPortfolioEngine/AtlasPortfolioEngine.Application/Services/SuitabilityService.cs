using AtlasPortfolioEngine.Application.DTOs;
using AtlasPortfolioEngine.Application.Interfaces;
using AtlasPortfolioEngine.Domain.Entities;
using AtlasPortfolioEngine.Domain.Enums;

namespace AtlasPortfolioEngine.Application.Services;

public class SuitabilityService : ISuitabilityService
{
    // Maximum equity concentration allowed per risk category
    private static readonly Dictionary<RiskCategory, decimal> MaxEquityConcentration = new()
    {
        { RiskCategory.Conservative, 0.30m },
        { RiskCategory.Balanced,     0.65m },
        { RiskCategory.Growth,       0.90m }
    };

    private static readonly HashSet<string> EquityAssetClasses = new()
    {
        "CanadianEquity", "USEquity", "InternationalEquity"
    };

    public SuitabilityResult Check(
        RiskProfile riskProfile,
        string assetClass,
        TransactionType transactionType,
        decimal amount,
        decimal totalPortfolioValue)
    {
        var validAssetClasses = new[] { "CanadianEquity", "USEquity", "InternationalEquity", "CanadianBonds", "GlobalBonds", "Cash" };
            if (!validAssetClasses.Contains(assetClass))
        return SuitabilityResult.Rejected(
            "INVALID_ASSET_CLASS",
            $"Invalid asset class '{assetClass}'. Valid values: {string.Join(", ", validAssetClasses)}"
        );
        // Only check Buy transactions — sells are always permitted
        if (transactionType != TransactionType.Buy)
            return SuitabilityResult.Approved();

        if (totalPortfolioValue <= 0)
            return SuitabilityResult.Approved(); // First trade, no concentration risk yet

        var isEquity = EquityAssetClasses.Contains(assetClass);

        if (!isEquity)
            return SuitabilityResult.Approved(); // Bonds and cash always suitable

        // Check if this buy would breach equity concentration limit
        var newEquityWeight = amount / (totalPortfolioValue + amount);
        var maxAllowed = MaxEquityConcentration[riskProfile.Category];

        if (newEquityWeight > maxAllowed)
            return SuitabilityResult.Rejected(
                "EQUITY_CONCENTRATION_BREACH",
                $"Trade would result in {newEquityWeight:P0} equity concentration. " +
                $"Maximum allowed for {riskProfile.Category} profile is {maxAllowed:P0}."
            );

        // Conservative clients cannot buy high-risk equity at all if score is very low
        if (riskProfile.Category == RiskCategory.Conservative && riskProfile.Score < 15 && isEquity)
            return SuitabilityResult.Rejected(
                "UNSUITABLE_FOR_RISK_PROFILE",
                $"Client risk score of {riskProfile.Score} is too low to purchase equity instruments."
            );

        return SuitabilityResult.Approved();
    }
}
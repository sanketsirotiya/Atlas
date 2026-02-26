using AtlasPortfolioEngine.Application.Interfaces;
using AtlasPortfolioEngine.Domain.Enums;

namespace AtlasPortfolioEngine.Application.Services;

public class ModelPortfolioService : IModelPortfolioService
{
    // Target allocations by asset class per risk category
    // These mirror how real robo-advisors define model portfolios
    private static readonly Dictionary<RiskCategory, Dictionary<string, decimal>> Models = new()
    {
        [RiskCategory.Conservative] = new()
        {
            { "CanadianEquity",      0.10m },
            { "USEquity",            0.10m },
            { "InternationalEquity", 0.05m },
            { "CanadianBonds",       0.50m },
            { "GlobalBonds",         0.20m },
            { "Cash",                0.05m }
        },
        [RiskCategory.Balanced] = new()
        {
            { "CanadianEquity",      0.20m },
            { "USEquity",            0.25m },
            { "InternationalEquity", 0.15m },
            { "CanadianBonds",       0.25m },
            { "GlobalBonds",         0.10m },
            { "Cash",                0.05m }
        },
        [RiskCategory.Growth] = new()
        {
            { "CanadianEquity",      0.30m },
            { "USEquity",            0.35m },
            { "InternationalEquity", 0.20m },
            { "CanadianBonds",       0.10m },
            { "GlobalBonds",         0.05m },
            { "Cash",                0.00m }
        }
    };

    public Dictionary<string, decimal> GetTargetAllocations(RiskCategory category)
    {
        if (!Models.TryGetValue(category, out var allocations))
            throw new ArgumentException($"No model portfolio defined for category: {category}");

        return allocations;
    }
}
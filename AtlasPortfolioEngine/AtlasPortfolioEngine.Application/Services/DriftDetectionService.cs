using AtlasPortfolioEngine.Application.DTOs;
using AtlasPortfolioEngine.Application.Interfaces;
using AtlasPortfolioEngine.Domain.Entities;

namespace AtlasPortfolioEngine.Application.Services;

public class DriftDetectionService : IDriftDetectionService
{
    // Industry standard drift threshold is 5%
    private const decimal DriftThreshold = 0.05m;

    public IEnumerable<DriftResult> Detect(Account account, Dictionary<string, decimal> targetAllocations)
    {
        var totalValue = account.Holdings.Sum(h => h.MarketValue);

        if (totalValue == 0)
            return targetAllocations.Select(t => new DriftResult
            {
                AssetClass = t.Key,
                TargetWeight = t.Value,
                ActualWeight = 0,
                RequiresRebalancing = t.Value > 0
            });

        // Group holdings by asset class and calculate actual weights
        var actualWeights = account.Holdings
            .GroupBy(h => h.AssetClass)
            .ToDictionary(
                g => g.Key,
                g => g.Sum(h => h.MarketValue) / totalValue
            );

        var results = new List<DriftResult>();

        foreach (var (assetClass, targetWeight) in targetAllocations)
        {
            actualWeights.TryGetValue(assetClass, out var actualWeight);

            results.Add(new DriftResult
            {
                AssetClass = assetClass,
                TargetWeight = targetWeight,
                ActualWeight = actualWeight,
                RequiresRebalancing = Math.Abs(actualWeight - targetWeight) > DriftThreshold
            });
        }

        return results;
    }
}
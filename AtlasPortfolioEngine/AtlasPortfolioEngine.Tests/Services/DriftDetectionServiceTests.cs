using AtlasPortfolioEngine.Application.Services;
using AtlasPortfolioEngine.Domain.Entities;
using FluentAssertions;

namespace AtlasPortfolioEngine.Tests.Services;

public class DriftDetectionServiceTests
{
    private readonly DriftDetectionService _sut = new();

    private static Account CreateAccountWithHoldings()
    {
        var account = Account.Create(Guid.NewGuid(), 0);

        // Use reflection to add holdings since _holdings is private
        var holdingsField = typeof(Account)
            .GetField("_holdings", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)!;

        var holdings = (List<Holding>)holdingsField.GetValue(account)!;

        holdings.Add(Holding.Create(account.Id, "XIC",  "CanadianEquity",      100m, 50m)); // $5,000
        holdings.Add(Holding.Create(account.Id, "XUS",  "USEquity",            100m, 50m)); // $5,000
        holdings.Add(Holding.Create(account.Id, "XBB",  "CanadianBonds",       100m, 50m)); // $5,000
        holdings.Add(Holding.Create(account.Id, "XGBO", "GlobalBonds",         100m, 50m)); // $5,000

        return account;
    }

    [Fact]
    public void Detect_WhenPortfolioMatchesTarget_ShouldNotRequireRebalancing()
    {
        var account = CreateAccountWithHoldings();

        // Each asset class is exactly 25%
        var targetAllocations = new Dictionary<string, decimal>
        {
            { "CanadianEquity", 0.25m },
            { "USEquity",       0.25m },
            { "CanadianBonds",  0.25m },
            { "GlobalBonds",    0.25m }
        };

        var results = _sut.Detect(account, targetAllocations).ToList();

        results.Should().OnlyContain(r => !r.RequiresRebalancing);
    }

    [Fact]
    public void Detect_WhenDriftExceedsThreshold_ShouldRequireRebalancing()
    {
        var account = CreateAccountWithHoldings();

        // Target says 50% CanadianEquity but actual is 25% — big drift
        var targetAllocations = new Dictionary<string, decimal>
        {
            { "CanadianEquity", 0.50m },
            { "USEquity",       0.20m },
            { "CanadianBonds",  0.20m },
            { "GlobalBonds",    0.10m }
        };

        var results = _sut.Detect(account, targetAllocations).ToList();

        results.Should().Contain(r => r.AssetClass == "CanadianEquity" && r.RequiresRebalancing);
    }

    [Fact]
    public void Detect_WithEmptyAccount_ShouldFlagAllAssetsAsRequiringRebalancing()
    {
        var emptyAccount = Account.Create(Guid.NewGuid(), 0);

        var targetAllocations = new Dictionary<string, decimal>
        {
            { "CanadianEquity", 0.60m },
            { "CanadianBonds",  0.40m }
        };

        var results = _sut.Detect(emptyAccount, targetAllocations).ToList();

        results.Should().OnlyContain(r => r.RequiresRebalancing);
    }

    [Fact]
    public void Detect_ShouldCorrectlyCalculateActualWeights()
    {
        var account = CreateAccountWithHoldings(); // 4 holdings, $5k each = $20k total, each 25%

        var targetAllocations = new Dictionary<string, decimal>
        {
            { "CanadianEquity", 0.25m },
            { "USEquity",       0.25m },
            { "CanadianBonds",  0.25m },
            { "GlobalBonds",    0.25m }
        };

        var results = _sut.Detect(account, targetAllocations).ToList();

        results.Should().OnlyContain(r => r.ActualWeight == 0.25m);
    }
}
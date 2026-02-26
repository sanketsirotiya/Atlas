using AtlasPortfolioEngine.Application.DTOs;
using AtlasPortfolioEngine.Application.Services;
using AtlasPortfolioEngine.Domain.Entities;
using AtlasPortfolioEngine.Domain.Enums;
using FluentAssertions;

namespace AtlasPortfolioEngine.Tests.Services;

public class RebalancingServiceTests
{
    private readonly RebalancingService _sut = new();

    private static Account CreateAccountWithHoldings()
    {
        var account = Account.Create(Guid.NewGuid(), 0);

        var holdingsField = typeof(Account)
            .GetField("_holdings", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)!;

        var holdings = (List<Holding>)holdingsField.GetValue(account)!;

        holdings.Add(Holding.Create(account.Id, "XIC",  "CanadianEquity", 100m, 50m)); // $5,000
        holdings.Add(Holding.Create(account.Id, "XBB",  "CanadianBonds",  100m, 50m)); // $5,000

        return account;
    }

    [Fact]
    public void GenerateOrders_WhenRebalancingNeeded_ShouldReturnOrders()
    {
        var account = CreateAccountWithHoldings();

        var driftResults = new List<DriftResult>
        {
            new() { AssetClass = "CanadianEquity", TargetWeight = 0.70m, ActualWeight = 0.50m, RequiresRebalancing = true },
            new() { AssetClass = "CanadianBonds",  TargetWeight = 0.30m, ActualWeight = 0.50m, RequiresRebalancing = true }
        };

        var orders = _sut.GenerateOrders(account, driftResults).ToList();

        orders.Should().NotBeEmpty();
        orders.Should().Contain(o => o.OrderType == TransactionType.Buy);
        orders.Should().Contain(o => o.OrderType == TransactionType.Sell);
    }

    [Fact]
    public void GenerateOrders_SellsShouldComeBeforeBuys()
    {
        var account = CreateAccountWithHoldings();

        var driftResults = new List<DriftResult>
        {
            new() { AssetClass = "CanadianEquity", TargetWeight = 0.70m, ActualWeight = 0.50m, RequiresRebalancing = true },
            new() { AssetClass = "CanadianBonds",  TargetWeight = 0.30m, ActualWeight = 0.50m, RequiresRebalancing = true }
        };

        var orders = _sut.GenerateOrders(account, driftResults).ToList();

        var firstSellIndex = orders.FindIndex(o => o.OrderType == TransactionType.Sell);
        var firstBuyIndex  = orders.FindIndex(o => o.OrderType == TransactionType.Buy);

        firstSellIndex.Should().BeLessThan(firstBuyIndex,
            because: "sells must be executed before buys to free up cash");
    }

    [Fact]
    public void GenerateOrders_WithEmptyAccount_ShouldThrowInvalidOperationException()
    {
        var emptyAccount = Account.Create(Guid.NewGuid(), 0);
        var driftResults = new List<DriftResult>
        {
            new() { AssetClass = "CanadianEquity", TargetWeight = 0.60m, ActualWeight = 0.0m, RequiresRebalancing = true }
        };

        var act = () => _sut.GenerateOrders(emptyAccount, driftResults).ToList();

        act.Should().Throw<InvalidOperationException>()
            .WithMessage("Cannot rebalance an empty account.");
    }

    [Fact]
    public void GenerateOrders_ShouldOnlyIncludeDriftedAssets()
    {
        var account = CreateAccountWithHoldings();

        var driftResults = new List<DriftResult>
        {
            new() { AssetClass = "CanadianEquity", TargetWeight = 0.70m, ActualWeight = 0.50m, RequiresRebalancing = true },
            new() { AssetClass = "CanadianBonds",  TargetWeight = 0.50m, ActualWeight = 0.50m, RequiresRebalancing = false }
        };

        var orders = _sut.GenerateOrders(account, driftResults).ToList();

        orders.Should().OnlyContain(o => o.AssetClass == "CanadianEquity",
            because: "CanadianBonds is within threshold and should not generate an order");
    }
}
using AtlasPortfolioEngine.Application.Services;
using AtlasPortfolioEngine.Domain.Entities;
using AtlasPortfolioEngine.Domain.Enums;
using FluentAssertions;

namespace AtlasPortfolioEngine.Tests.Services;

public class SuitabilityServiceTests
{
    private readonly SuitabilityService _sut = new();

    private static RiskProfile CreateRiskProfile(int score)
        => RiskProfile.Create(Guid.NewGuid(), score);

    [Fact]
    public void Check_BuyEquity_WithinConcentrationLimit_ShouldApprove()
    {
        var riskProfile = CreateRiskProfile(72); // Growth
        
        var result = _sut.Check(
            riskProfile,
            assetClass: "USEquity",
            transactionType: TransactionType.Buy,
            amount: 5000m,
            totalPortfolioValue: 32000m
        );

        result.IsApproved.Should().BeTrue();
        result.ReasonCode.Should().Be("APPROVED");
    }

    [Fact]
    public void Check_BuyEquity_BreachingConcentrationLimit_ShouldReject()
    {
        var riskProfile = CreateRiskProfile(72); // Growth, max 90% equity

        var result = _sut.Check(
            riskProfile,
            assetClass: "USEquity",
            transactionType: TransactionType.Buy,
            amount: 500000m,
            totalPortfolioValue: 32000m
        );

        result.IsApproved.Should().BeFalse();
        result.ReasonCode.Should().Be("EQUITY_CONCENTRATION_BREACH");
        result.Message.Should().Contain("90%");
    }

    [Fact]
    public void Check_ConservativeClient_LowScore_BuyingEquity_ShouldReject()
    {
        var riskProfile = CreateRiskProfile(10); // Conservative, score < 15

        var result = _sut.Check(
            riskProfile,
            assetClass: "CanadianEquity",
            transactionType: TransactionType.Buy,
            amount: 1000m,
            totalPortfolioValue: 50000m
        );

        result.IsApproved.Should().BeFalse();
        result.ReasonCode.Should().Be("UNSUITABLE_FOR_RISK_PROFILE");
    }

    [Fact]
    public void Check_SellTransaction_ShouldAlwaysApprove()
    {
        var riskProfile = CreateRiskProfile(10); // Most conservative profile

        var result = _sut.Check(
            riskProfile,
            assetClass: "USEquity",
            transactionType: TransactionType.Sell,
            amount: 999999m,
            totalPortfolioValue: 32000m
        );

        result.IsApproved.Should().BeTrue();
        result.ReasonCode.Should().Be("APPROVED");
    }

    [Fact]
    public void Check_BuyBonds_ShouldAlwaysApprove()
    {
        var riskProfile = CreateRiskProfile(10); // Most conservative profile

        var result = _sut.Check(
            riskProfile,
            assetClass: "CanadianBonds",
            transactionType: TransactionType.Buy,
            amount: 100000m,
            totalPortfolioValue: 32000m
        );

        result.IsApproved.Should().BeTrue();
        result.ReasonCode.Should().Be("APPROVED");
    }

    [Fact]
    public void Check_FirstTrade_ZeroPortfolioValue_ShouldApprove()
    {
        var riskProfile = CreateRiskProfile(50); // Balanced

        var result = _sut.Check(
            riskProfile,
            assetClass: "USEquity",
            transactionType: TransactionType.Buy,
            amount: 10000m,
            totalPortfolioValue: 0m // no holdings yet
        );

        result.IsApproved.Should().BeTrue();
        result.ReasonCode.Should().Be("APPROVED");
    }
}
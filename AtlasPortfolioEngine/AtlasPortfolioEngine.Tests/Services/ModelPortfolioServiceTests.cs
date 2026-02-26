using AtlasPortfolioEngine.Application.Services;
using AtlasPortfolioEngine.Domain.Enums;
using FluentAssertions;

namespace AtlasPortfolioEngine.Tests.Services;

public class ModelPortfolioServiceTests
{
    private readonly ModelPortfolioService _sut = new();

    [Fact]
    public void GetTargetAllocations_ForConservative_ShouldHaveMajorityBonds()
    {
        var allocations = _sut.GetTargetAllocations(RiskCategory.Conservative);

        var bondWeight = allocations["CanadianBonds"] + allocations["GlobalBonds"];
        var equityWeight = allocations["CanadianEquity"] + allocations["USEquity"] + allocations["InternationalEquity"];

        bondWeight.Should().BeGreaterThan(equityWeight);
    }

    [Fact]
    public void GetTargetAllocations_ForGrowth_ShouldHaveMajorityEquity()
    {
        var allocations = _sut.GetTargetAllocations(RiskCategory.Growth);

        var equityWeight = allocations["CanadianEquity"] + allocations["USEquity"] + allocations["InternationalEquity"];
        var bondWeight = allocations["CanadianBonds"] + allocations["GlobalBonds"];

        equityWeight.Should().BeGreaterThan(bondWeight);
    }

    [Fact]
    public void GetTargetAllocations_ShouldAlwaysSumToOne()
    {
        foreach (RiskCategory category in Enum.GetValues<RiskCategory>())
        {
            var allocations = _sut.GetTargetAllocations(category);
            var total = allocations.Values.Sum();

            total.Should().BeApproximately(1.0m, 0.001m,
                because: $"{category} allocations must sum to 100%");
        }
    }

    [Fact]
    public void GetTargetAllocations_WithInvalidCategory_ShouldThrowArgumentException()
    {
        var invalidCategory = (RiskCategory)99;

        var act = () => _sut.GetTargetAllocations(invalidCategory);

        act.Should().Throw<ArgumentException>()
            .WithMessage("*No model portfolio defined*");
    }

    [Theory]
    [InlineData(RiskCategory.Conservative)]
    [InlineData(RiskCategory.Balanced)]
    [InlineData(RiskCategory.Growth)]
    public void GetTargetAllocations_ShouldReturnAllAssetClasses(RiskCategory category)
    {
        var allocations = _sut.GetTargetAllocations(category);

        allocations.Should().ContainKey("CanadianEquity");
        allocations.Should().ContainKey("USEquity");
        allocations.Should().ContainKey("InternationalEquity");
        allocations.Should().ContainKey("CanadianBonds");
        allocations.Should().ContainKey("GlobalBonds");
    }
}
using AtlasPortfolioEngine.Application.Services;
using AtlasPortfolioEngine.Domain.Enums;
using FluentAssertions;

namespace AtlasPortfolioEngine.Tests.Services;

public class RiskProfileServiceTests
{
    private readonly RiskProfileService _sut = new();

    [Fact]
    public void Assess_WithLowAnswers_ShouldReturnConservativeProfile()
    {
        var clientId = Guid.NewGuid();
        var answers = new List<int> { 1, 1, 1, 1, 1 }; // all lowest

        var result = _sut.Assess(clientId, answers);

        result.Category.Should().Be(RiskCategory.Conservative);
        result.Score.Should().BeLessThanOrEqualTo(33);
        result.ClientId.Should().Be(clientId);
    }

    [Fact]
    public void Assess_WithMidAnswers_ShouldReturnBalancedProfile()
    {
        var clientId = Guid.NewGuid();
        var answers = new List<int> { 3, 3, 3, 3, 3 }; // all mid

        var result = _sut.Assess(clientId, answers);

        result.Category.Should().Be(RiskCategory.Balanced);
        result.Score.Should().BeInRange(34, 66);
    }

    [Fact]
    public void Assess_WithHighAnswers_ShouldReturnGrowthProfile()
    {
        var clientId = Guid.NewGuid();
        var answers = new List<int> { 5, 5, 5, 5, 5 }; // all highest

        var result = _sut.Assess(clientId, answers);

        result.Category.Should().Be(RiskCategory.Growth);
        result.Score.Should().BeGreaterThanOrEqualTo(67);
    }

    [Fact]
    public void Assess_WithEmptyAnswers_ShouldThrowArgumentException()
    {
        var clientId = Guid.NewGuid();
        var answers = new List<int>();

        var act = () => _sut.Assess(clientId, answers);

        act.Should().Throw<ArgumentException>()
            .WithMessage("Questionnaire answers are required.");
    }

    [Fact]
    public void Assess_WithOutOfRangeAnswers_ShouldThrowArgumentException()
    {
        var clientId = Guid.NewGuid();
        var answers = new List<int> { 1, 2, 6 }; // 6 is invalid

        var act = () => _sut.Assess(clientId, answers);

        act.Should().Throw<ArgumentException>()
            .WithMessage("Each answer must be between 1 and 5.");
    }
}
using AtlasPortfolioEngine.Domain.Entities;

namespace AtlasPortfolioEngine.Application.Interfaces;

public interface IRiskProfileService
{
    RiskProfile Assess(Guid clientId, IEnumerable<int> questionnaireAnswers);
}
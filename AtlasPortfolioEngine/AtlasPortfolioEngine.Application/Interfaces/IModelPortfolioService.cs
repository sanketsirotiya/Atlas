using AtlasPortfolioEngine.Domain.Enums;

namespace AtlasPortfolioEngine.Application.Interfaces;

public interface IModelPortfolioService
{
    Dictionary<string, decimal> GetTargetAllocations(RiskCategory category);
}
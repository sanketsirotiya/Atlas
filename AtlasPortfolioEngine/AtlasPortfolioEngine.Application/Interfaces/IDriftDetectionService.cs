using AtlasPortfolioEngine.Application.DTOs;
using AtlasPortfolioEngine.Domain.Entities;

namespace AtlasPortfolioEngine.Application.Interfaces;

public interface IDriftDetectionService
{
    IEnumerable<DriftResult> Detect(Account account, Dictionary<string, decimal> targetAllocations);
}
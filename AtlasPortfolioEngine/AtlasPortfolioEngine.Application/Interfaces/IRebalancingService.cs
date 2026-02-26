using AtlasPortfolioEngine.Application.DTOs;
using AtlasPortfolioEngine.Domain.Entities;

namespace AtlasPortfolioEngine.Application.Interfaces;

public interface IRebalancingService
{
    IEnumerable<RebalanceOrder> GenerateOrders(Account account, IEnumerable<DriftResult> driftResults);
}
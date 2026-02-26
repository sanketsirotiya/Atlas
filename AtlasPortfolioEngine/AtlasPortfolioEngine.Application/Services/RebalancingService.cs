using AtlasPortfolioEngine.Application.DTOs;
using AtlasPortfolioEngine.Application.Interfaces;
using AtlasPortfolioEngine.Domain.Entities;
using AtlasPortfolioEngine.Domain.Enums;

namespace AtlasPortfolioEngine.Application.Services;

public class RebalancingService : IRebalancingService
{
    public IEnumerable<RebalanceOrder> GenerateOrders(Account account, IEnumerable<DriftResult> driftResults)
    {
        var totalValue = account.Holdings.Sum(h => h.MarketValue) + account.CashBalance;

        if (totalValue == 0)
            throw new InvalidOperationException("Cannot rebalance an empty account.");

        var orders = new List<RebalanceOrder>();

        foreach (var drift in driftResults.Where(d => d.RequiresRebalancing))
        {
            var targetValue = drift.TargetWeight * totalValue;
            var actualValue = drift.ActualWeight * totalValue;
            var delta = targetValue - actualValue;

            // Find a representative symbol for this asset class
            var holding = account.Holdings.FirstOrDefault(h => h.AssetClass == drift.AssetClass);
            var symbol = holding?.Symbol ?? drift.AssetClass; // fallback to asset class name

            orders.Add(new RebalanceOrder
            {
                AssetClass = drift.AssetClass,
                Symbol = symbol,
                OrderType = delta > 0 ? TransactionType.Buy : TransactionType.Sell,
                Amount = Math.Abs(delta)
            });
        }

        // Sells first — free up cash before buying
        return orders.OrderBy(o => o.OrderType == TransactionType.Sell ? 0 : 1);
    }
}
using AtlasPortfolioEngine.Domain.Enums;

namespace AtlasPortfolioEngine.Application.DTOs;

public class RebalanceOrder
{
    public string AssetClass { get; set; } = string.Empty;
    public string Symbol { get; set; } = string.Empty;
    public TransactionType OrderType { get; set; }
    public decimal Amount { get; set; } // Dollar amount to buy or sell
}
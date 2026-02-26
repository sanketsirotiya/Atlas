namespace AtlasPortfolioEngine.Domain.Entities;

public class Holding
{
    public Guid Id { get; private set; }
    public Guid AccountId { get; private set; }
    public string Symbol { get; private set; } = string.Empty;
    public string AssetClass { get; private set; } = string.Empty;
    public decimal Quantity { get; private set; }
    public decimal AverageCost { get; private set; }
    public decimal CurrentPrice { get; private set; }

    public decimal MarketValue => Quantity * CurrentPrice;
    public decimal UnrealizedGainLoss => (CurrentPrice - AverageCost) * Quantity;

    private Holding() { } // EF Core

    public static Holding Create(Guid accountId, string symbol, string assetClass, decimal quantity, decimal price)
    {
        if (quantity <= 0) throw new ArgumentException("Quantity must be positive.");
        if (price <= 0) throw new ArgumentException("Price must be positive.");

        return new Holding
        {
            Id = Guid.NewGuid(),
            AccountId = accountId,
            Symbol = symbol,
            AssetClass = assetClass,
            Quantity = quantity,
            AverageCost = price,
            CurrentPrice = price
        };
    }

    public void UpdatePrice(decimal newPrice)
    {
        if (newPrice <= 0) throw new ArgumentException("Price must be positive.");
        CurrentPrice = newPrice;
    }

    public void AddShares(decimal quantity, decimal price)
    {
        if (quantity <= 0) throw new ArgumentException("Quantity must be positive.");
        // Weighted average cost calculation
        AverageCost = ((AverageCost * Quantity) + (price * quantity)) / (Quantity + quantity);
        Quantity += quantity;
    }

    public void RemoveShares(decimal quantity)
    {
        if (quantity <= 0) throw new ArgumentException("Quantity must be positive.");
        if (quantity > Quantity) throw new InvalidOperationException("Cannot sell more shares than held.");
        Quantity -= quantity;
    }
}
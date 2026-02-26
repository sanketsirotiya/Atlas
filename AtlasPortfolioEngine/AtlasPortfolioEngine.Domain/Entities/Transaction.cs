using AtlasPortfolioEngine.Domain.Enums;

namespace AtlasPortfolioEngine.Domain.Entities;

public class Transaction
{
    public Guid Id { get; private set; }
    public Guid AccountId { get; private set; }
    public string Symbol { get; private set; } = string.Empty;
    public TransactionType Type { get; private set; }
    public decimal Quantity { get; private set; }
    public decimal Price { get; private set; }
    public decimal TotalAmount => Quantity * Price;
    public DateTime ExecutedAt { get; private set; }

    private Transaction() { } // EF Core

    public static Transaction Record(Guid accountId, string symbol, TransactionType type, decimal quantity, decimal price)
    {
        return new Transaction
        {
            Id = Guid.NewGuid(),
            AccountId = accountId,
            Symbol = symbol,
            Type = type,
            Quantity = quantity,
            Price = price,
            ExecutedAt = DateTime.UtcNow
        };
    }
}
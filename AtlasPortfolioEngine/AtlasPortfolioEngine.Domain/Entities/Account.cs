namespace AtlasPortfolioEngine.Domain.Entities;

public class Account
{
    public Guid Id { get; private set; }
    public Guid ClientId { get; private set; }
    public decimal CashBalance { get; private set; }
    public DateTime CreatedAt { get; private set; }

    private List<Holding> _holdings = new();
    private List<Transaction> _transactions = new();

    public IReadOnlyCollection<Holding> Holdings => _holdings.AsReadOnly();
    public IReadOnlyCollection<Transaction> Transactions => _transactions.AsReadOnly();

    private Account() { } // EF Core

    public static Account Create(Guid clientId, decimal initialCash = 0)
    {
        if (initialCash < 0) throw new ArgumentException("Initial cash cannot be negative.");

        return new Account
        {
            Id = Guid.NewGuid(),
            ClientId = clientId,
            CashBalance = initialCash,
            CreatedAt = DateTime.UtcNow
        };
    }

    public void Deposit(decimal amount)
    {
        if (amount <= 0) throw new ArgumentException("Deposit amount must be positive.");
        CashBalance += amount;
    }
}
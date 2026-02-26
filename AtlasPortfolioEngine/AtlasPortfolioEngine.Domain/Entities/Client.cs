namespace AtlasPortfolioEngine.Domain.Entities;

public class Client
{
    public Guid Id { get; private set; }
    public string FullName { get; private set; } = string.Empty;
    public string Email { get; private set; } = string.Empty;
    public DateOnly DateOfBirth { get; private set; }
    public DateTime CreatedAt { get; private set; }

    public RiskProfile? RiskProfile { get; private set; }
    public Account? Account { get; private set; }

    private Client() { } // EF Core

    public static Client Create(string fullName, string email, DateOnly dateOfBirth)
    {
        if (string.IsNullOrWhiteSpace(fullName)) throw new ArgumentException("Full name is required.");
        if (string.IsNullOrWhiteSpace(email)) throw new ArgumentException("Email is required.");

        return new Client
        {
            Id = Guid.NewGuid(),
            FullName = fullName,
            Email = email,
            DateOfBirth = dateOfBirth,
            CreatedAt = DateTime.UtcNow
        };
    }
}
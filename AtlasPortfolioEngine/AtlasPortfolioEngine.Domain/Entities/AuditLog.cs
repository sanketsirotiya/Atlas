namespace AtlasPortfolioEngine.Domain.Entities;

public class AuditLog
{
    public Guid Id { get; private set; }
    public Guid ClientId { get; private set; }
    public string Action { get; private set; } = string.Empty;
    public string Details { get; private set; } = string.Empty;
    public DateTime OccurredAt { get; private set; }

    private AuditLog() { } // EF Core

    public static AuditLog Record(Guid clientId, string action, string details)
    {
        if (string.IsNullOrWhiteSpace(action)) throw new ArgumentException("Action is required.");

        return new AuditLog
        {
            Id = Guid.NewGuid(),
            ClientId = clientId,
            Action = action,
            Details = details,
            OccurredAt = DateTime.UtcNow
        };
    }
}
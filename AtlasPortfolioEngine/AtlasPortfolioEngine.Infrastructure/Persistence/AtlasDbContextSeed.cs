using AtlasPortfolioEngine.Domain.Entities;
using AtlasPortfolioEngine.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace AtlasPortfolioEngine.Infrastructure.Persistence;

public static class AtlasDbContextSeed
{
    public static async Task SeedAsync(AtlasDbContext context)
    {
        if (await context.Clients.AnyAsync()) return; // Already seeded

        // --- Client ---
        var client = Client.Create(
            "Sarah Mitchell",
            "sarah.mitchell@email.com",
            new DateOnly(1985, 6, 15)
        );

        await context.Clients.AddAsync(client);
        await context.SaveChangesAsync();

        // --- Risk Profile (score 72 = Growth) ---
        var riskProfile = RiskProfile.Create(client.Id, 72);
        await context.RiskProfiles.AddAsync(riskProfile);
        await context.SaveChangesAsync();

        // --- Account ---
        var account = Account.Create(client.Id, 5000.00m); // $5k cash
        await context.Accounts.AddAsync(account);
        await context.SaveChangesAsync();

        // --- Holdings (Canadian ETFs — realistic for Nestwealth) ---
        var holdings = new List<Holding>
        {
            Holding.Create(account.Id, "XIC",  "CanadianEquity",      150m, 32.50m),
            Holding.Create(account.Id, "XUS",  "USEquity",            200m, 58.75m),
            Holding.Create(account.Id, "XEF",  "InternationalEquity", 100m, 35.20m),
            Holding.Create(account.Id, "XBB",  "CanadianBonds",       300m, 29.10m),
            Holding.Create(account.Id, "XGBO", "GlobalBonds",         150m, 24.80m)
        };

        await context.Holdings.AddRangeAsync(holdings);
        await context.SaveChangesAsync();

        // --- Transactions (buy history) ---
        var transactions = new List<Transaction>
        {
            Transaction.Record(account.Id, "XIC",  TransactionType.Buy, 150m, 32.50m),
            Transaction.Record(account.Id, "XUS",  TransactionType.Buy, 200m, 58.75m),
            Transaction.Record(account.Id, "XEF",  TransactionType.Buy, 100m, 35.20m),
            Transaction.Record(account.Id, "XBB",  TransactionType.Buy, 300m, 29.10m),
            Transaction.Record(account.Id, "XGBO", TransactionType.Buy, 150m, 24.80m)
        };

        await context.Transactions.AddRangeAsync(transactions);
        await context.SaveChangesAsync();

        // --- Audit Log ---
        var audit = AuditLog.Record(client.Id, "ACCOUNT_CREATED", 
            $"Client Sarah Mitchell onboarded. Risk category: Growth. Initial cash: $5,000.");
        await context.AuditLogs.AddAsync(audit);
        await context.SaveChangesAsync();
    }
}
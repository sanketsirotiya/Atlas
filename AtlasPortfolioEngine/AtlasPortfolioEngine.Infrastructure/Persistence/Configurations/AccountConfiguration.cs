using AtlasPortfolioEngine.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AtlasPortfolioEngine.Infrastructure.Persistence.Configurations;

public class AccountConfiguration : IEntityTypeConfiguration<Account>
{
    public void Configure(EntityTypeBuilder<Account> builder)
    {
        builder.HasKey(a => a.Id);
        builder.Property(a => a.CashBalance).HasPrecision(18, 2);

        builder.HasMany(a => a.Holdings)
               .WithOne()
               .HasForeignKey(h => h.AccountId);

        builder.HasMany(a => a.Transactions)
               .WithOne()
               .HasForeignKey(t => t.AccountId);
    }
}
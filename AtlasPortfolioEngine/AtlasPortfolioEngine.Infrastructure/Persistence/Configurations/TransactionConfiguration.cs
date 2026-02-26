using AtlasPortfolioEngine.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AtlasPortfolioEngine.Infrastructure.Persistence.Configurations;

public class TransactionConfiguration : IEntityTypeConfiguration<Transaction>
{
    public void Configure(EntityTypeBuilder<Transaction> builder)
    {
        builder.HasKey(t => t.Id);
        builder.Property(t => t.Symbol).IsRequired().HasMaxLength(20);
        builder.Property(t => t.Quantity).HasPrecision(18, 6);
        builder.Property(t => t.Price).HasPrecision(18, 2);

        builder.Ignore(t => t.TotalAmount);
    }
}

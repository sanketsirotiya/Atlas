using AtlasPortfolioEngine.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AtlasPortfolioEngine.Infrastructure.Persistence.Configurations;

public class HoldingConfiguration : IEntityTypeConfiguration<Holding>
{
    public void Configure(EntityTypeBuilder<Holding> builder)
    {
        builder.HasKey(h => h.Id);
        builder.Property(h => h.Symbol).IsRequired().HasMaxLength(20);
        builder.Property(h => h.AssetClass).IsRequired().HasMaxLength(100);
        builder.Property(h => h.Quantity).HasPrecision(18, 6);
        builder.Property(h => h.AverageCost).HasPrecision(18, 2);
        builder.Property(h => h.CurrentPrice).HasPrecision(18, 2);

        // Computed columns — ignored by EF, calculated in domain
        builder.Ignore(h => h.MarketValue);
        builder.Ignore(h => h.UnrealizedGainLoss);
    }
}
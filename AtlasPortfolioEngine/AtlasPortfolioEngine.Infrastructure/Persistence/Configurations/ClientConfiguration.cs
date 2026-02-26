using AtlasPortfolioEngine.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AtlasPortfolioEngine.Infrastructure.Persistence.Configurations;

public class ClientConfiguration : IEntityTypeConfiguration<Client>
{
    public void Configure(EntityTypeBuilder<Client> builder)
    {
        builder.HasKey(c => c.Id);
        builder.Property(c => c.FullName).IsRequired().HasMaxLength(200);
        builder.Property(c => c.Email).IsRequired().HasMaxLength(200);

        builder.HasOne(c => c.RiskProfile)
               .WithOne()
               .HasForeignKey<RiskProfile>(r => r.ClientId);

        builder.HasOne(c => c.Account)
               .WithOne()
               .HasForeignKey<Account>(a => a.ClientId);
    }
}
using AtlasPortfolioEngine.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AtlasPortfolioEngine.Infrastructure.Persistence.Configurations;

public class AuditLogConfiguration : IEntityTypeConfiguration<AuditLog>
{
    public void Configure(EntityTypeBuilder<AuditLog> builder)
    {
        builder.HasKey(a => a.Id);
        builder.Property(a => a.Action).IsRequired().HasMaxLength(100);
        builder.Property(a => a.Details).HasMaxLength(2000);

        // Audit logs are immutable — no updates allowed
        builder.ToTable(tb => tb.HasCheckConstraint("CK_AuditLog_Immutable", "1=1"));
    }
}
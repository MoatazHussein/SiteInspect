using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SiteInspect.Domain.Common.Numbering;

namespace SiteInspect.Infrastructure.Persistence.Configurations.Common;

internal sealed class NumberSeriesCounterConfiguration
    : ConcurrentAuditableEntityConfiguration<NumberSeriesCounter>
{
    public override void Configure(EntityTypeBuilder<NumberSeriesCounter> builder)
    {
        base.Configure(builder);

        builder.Property(counter => counter.SeriesName)
            .HasMaxLength(100)
            .IsRequired();

        builder.HasIndex(counter => new { counter.SeriesName, counter.Year })
            .IsUnique();
    }
}

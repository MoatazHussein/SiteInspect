using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SiteInspect.Domain.Inspections.InspectionTemplateAggregate;
using SiteInspect.Infrastructure.Persistence.Configurations.Common;

namespace SiteInspect.Infrastructure.Persistence.Configurations.Inspections;

internal sealed class InspectionTemplateItemConfiguration
    : BaseEntityConfiguration<InspectionTemplateItem>
{
    public override void Configure(EntityTypeBuilder<InspectionTemplateItem> builder)
    {
        base.Configure(builder);

        builder.Property(item => item.SectionName)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(item => item.Question)
            .HasMaxLength(1000)
            .IsRequired();

        builder.Property(item => item.DefaultSeverity)
            .HasConversion<string>()
            .HasMaxLength(16);

        builder.HasIndex(item => new { item.InspectionTemplateId, item.DisplayOrder })
            .IsUnique();
    }
}

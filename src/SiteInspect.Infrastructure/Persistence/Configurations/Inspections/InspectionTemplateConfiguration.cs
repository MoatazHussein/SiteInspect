using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SiteInspect.Domain.Inspections.InspectionTemplateAggregate;
using SiteInspect.Infrastructure.Persistence.Configurations.Common;

namespace SiteInspect.Infrastructure.Persistence.Configurations.Inspections;

internal sealed class InspectionTemplateConfiguration
    : AuditableEntityConfiguration<InspectionTemplate>
{
    public override void Configure(EntityTypeBuilder<InspectionTemplate> builder)
    {
        base.Configure(builder);

        builder.Property(template => template.Name)
            .HasMaxLength(200)
            .IsRequired();

        builder.HasIndex(template => new { template.Name, template.Version })
            .IsUnique();

        builder.HasMany(template => template.Items)
            .WithOne()
            .HasForeignKey(item => item.InspectionTemplateId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

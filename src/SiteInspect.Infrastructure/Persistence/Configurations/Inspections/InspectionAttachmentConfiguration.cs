using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SiteInspect.Domain.Inspections.InspectionAggregate;
using SiteInspect.Infrastructure.Persistence.Configurations.Common;

namespace SiteInspect.Infrastructure.Persistence.Configurations.Inspections;

internal sealed class InspectionAttachmentConfiguration
    : AuditableEntityConfiguration<InspectionAttachment>
{
    public override void Configure(EntityTypeBuilder<InspectionAttachment> builder)
    {
        base.Configure(builder);

        builder.Property(attachment => attachment.OriginalFileName)
            .HasMaxLength(255)
            .IsRequired();

        builder.Property(attachment => attachment.StoredFileName)
            .HasMaxLength(255)
            .IsRequired();

        builder.Property(attachment => attachment.ContentType)
            .HasMaxLength(100)
            .IsRequired();

        builder.HasIndex(attachment => attachment.ObservationId);
        builder.HasIndex(attachment => attachment.StoredFileName).IsUnique();
    }
}

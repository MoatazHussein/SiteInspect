using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SiteInspect.Domain.Inspections.InspectionAggregate;
using SiteInspect.Domain.Inspections.InspectionTemplateAggregate;
using SiteInspect.Infrastructure.Persistence.Configurations.Common;

namespace SiteInspect.Infrastructure.Persistence.Configurations.Inspections;

internal sealed class InspectionObservationConfiguration
    : BaseEntityConfiguration<InspectionObservation>
{
    public override void Configure(EntityTypeBuilder<InspectionObservation> builder)
    {
        base.Configure(builder);

        builder.Property(observation => observation.SectionNameSnapshot)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(observation => observation.QuestionSnapshot)
            .HasMaxLength(1000)
            .IsRequired();

        builder.Property(observation => observation.Outcome)
            .HasConversion<string>()
            .HasMaxLength(24);

        builder.Property(observation => observation.Severity)
            .HasConversion<string>()
            .HasMaxLength(16);

        builder.Property(observation => observation.Notes)
            .HasMaxLength(2000);

        builder.Property(observation => observation.ObservedAtUtc)
            .HasPrecision(7);

        builder.HasOne<InspectionTemplateItem>()
            .WithMany()
            .HasForeignKey(observation => observation.TemplateItemId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(observation => new { observation.InspectionId, observation.TemplateItemId })
            .IsUnique();

        builder.HasMany(observation => observation.Attachments)
            .WithOne()
            .HasForeignKey(attachment => attachment.ObservationId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

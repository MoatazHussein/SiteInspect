using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SiteInspect.Domain.Inspections.InspectionAggregate;
using SiteInspect.Domain.Inspections.InspectionTemplateAggregate;
using SiteInspect.Domain.Projects.ProjectAggregate;
using SiteInspect.Infrastructure.Identity.Entities;
using SiteInspect.Infrastructure.Persistence.Configurations.Common;

namespace SiteInspect.Infrastructure.Persistence.Configurations.Inspections;

internal sealed class InspectionConfiguration : ConcurrentAuditableEntityConfiguration<Inspection>
{
    public override void Configure(EntityTypeBuilder<Inspection> builder)
    {
        base.Configure(builder);

        builder.Property(inspection => inspection.Number)
            .HasMaxLength(32)
            .IsRequired();

        builder.Property(inspection => inspection.Status)
            .HasConversion<string>()
            .HasMaxLength(32);

        builder.Property(inspection => inspection.DueAtUtc).HasPrecision(7);
        builder.Property(inspection => inspection.StartedAtUtc).HasPrecision(7);
        builder.Property(inspection => inspection.SubmittedAtUtc).HasPrecision(7);
        builder.Property(inspection => inspection.CompletedAtUtc).HasPrecision(7);
        builder.Property(inspection => inspection.CancelledAtUtc).HasPrecision(7);
        builder.Property(inspection => inspection.LastDraftSavedAtUtc).HasPrecision(7);

        builder.Property(inspection => inspection.CancellationReason)
            .HasMaxLength(1000);

        builder.HasIndex(inspection => inspection.Number).IsUnique();
        builder.HasIndex(inspection => new { inspection.AssignedInspectorId, inspection.Status, inspection.DueAtUtc });
        builder.HasIndex(inspection => new { inspection.ProjectId, inspection.ProjectLocationId });

        builder.HasOne<Project>()
            .WithMany()
            .HasForeignKey(inspection => inspection.ProjectId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<ProjectLocation>()
            .WithMany()
            .HasForeignKey(inspection => inspection.ProjectLocationId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<InspectionTemplate>()
            .WithMany()
            .HasForeignKey(inspection => inspection.TemplateId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<ApplicationUser>()
            .WithMany()
            .HasForeignKey(inspection => inspection.AssignedInspectorId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(inspection => inspection.Observations)
            .WithOne()
            .HasForeignKey(observation => observation.InspectionId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

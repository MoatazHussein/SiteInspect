using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SiteInspect.Domain.CorrectiveActions.CorrectiveActionAggregate;
using SiteInspect.Domain.Inspections.InspectionAggregate;
using SiteInspect.Infrastructure.Identity.Entities;
using SiteInspect.Infrastructure.Persistence.Configurations.Common;

namespace SiteInspect.Infrastructure.Persistence.Configurations.CorrectiveActions;

internal sealed class CorrectiveActionConfiguration : ConcurrentAuditableEntityConfiguration<CorrectiveAction>
{
    public override void Configure(EntityTypeBuilder<CorrectiveAction> builder)
    {
        base.Configure(builder);
        builder.Property(action => action.Description).HasMaxLength(2000).IsRequired();
        builder.Property(action => action.Status).HasConversion<string>().HasMaxLength(24);
        builder.Property(action => action.ResolutionNotes).HasMaxLength(2000);
        builder.Property(action => action.RejectionReason).HasMaxLength(2000);
        builder.Property(action => action.DueAtUtc).HasPrecision(7);
        builder.Property(action => action.SubmittedAtUtc).HasPrecision(7);
        builder.Property(action => action.RejectedAtUtc).HasPrecision(7);
        builder.Property(action => action.ClosedAtUtc).HasPrecision(7);
        builder.HasIndex(action => action.ObservationId).IsUnique();
        builder.HasIndex(action => new { action.AssignedContractorId, action.Status, action.DueAtUtc });
        builder.HasOne<InspectionObservation>().WithMany()
            .HasForeignKey(action => new { action.InspectionId, action.ObservationId })
            .HasPrincipalKey(observation => new { observation.InspectionId, observation.Id })
            .OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<ApplicationUser>().WithMany()
            .HasForeignKey(action => action.AssignedContractorId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}


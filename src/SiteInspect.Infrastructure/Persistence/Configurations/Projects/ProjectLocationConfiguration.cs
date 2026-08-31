using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SiteInspect.Domain.Projects.ProjectAggregate;
using SiteInspect.Infrastructure.Persistence.Configurations.Common;

namespace SiteInspect.Infrastructure.Persistence.Configurations.Projects;

internal sealed class ProjectLocationConfiguration : AuditableEntityConfiguration<ProjectLocation>
{
    public override void Configure(EntityTypeBuilder<ProjectLocation> builder)
    {
        base.Configure(builder);

        builder.Property(location => location.Name)
            .HasMaxLength(200)
            .IsRequired();

        builder.HasOne<Project>()
            .WithMany()
            .HasForeignKey(location => location.ProjectId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<ProjectLocation>()
            .WithMany()
            .HasForeignKey(location => location.ParentLocationId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(location => new { location.ProjectId, location.ParentLocationId, location.Name })
            .IsUnique();
    }
}

using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using SiteInspect.Application.Common.Abstractions.Identity;
using SiteInspect.Application.Common.Abstractions.Persistence;
using SiteInspect.Application.Common.Exceptions;
using SiteInspect.Domain.Common.Entities;
using SiteInspect.Domain.Common.Numbering;
using SiteInspect.Domain.Inspections.InspectionAggregate;
using SiteInspect.Domain.Inspections.InspectionTemplateAggregate;
using SiteInspect.Domain.Projects.ProjectAggregate;
using SiteInspect.Infrastructure.Identity.Entities;

namespace SiteInspect.Infrastructure.Persistence;

public sealed class ApplicationDbContext(
    DbContextOptions<ApplicationDbContext> options,
    TimeProvider timeProvider,
    ICurrentUser currentUser)
    : IdentityDbContext<ApplicationUser, IdentityRole<Guid>, Guid>(options), IUnitOfWork
{
    public DbSet<Project> Projects => Set<Project>();

    public DbSet<NumberSeriesCounter> NumberSeriesCounters => Set<NumberSeriesCounter>();

    public DbSet<ProjectLocation> ProjectLocations => Set<ProjectLocation>();

    public DbSet<InspectionTemplate> InspectionTemplates => Set<InspectionTemplate>();

    public DbSet<InspectionTemplateItem> InspectionTemplateItems => Set<InspectionTemplateItem>();

    public DbSet<Inspection> Inspections => Set<Inspection>();

    public DbSet<InspectionObservation> InspectionObservations => Set<InspectionObservation>();

    public DbSet<InspectionAttachment> InspectionAttachments => Set<InspectionAttachment>();

    internal DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);
    }

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        ApplyAuditMetadata();

        try
        {
            return await base.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateConcurrencyException exception)
        {
            throw new PersistenceConcurrencyException(exception);
        }
    }

    private void ApplyAuditMetadata()
    {
        var occurredAtUtc = timeProvider.GetUtcNow();

        foreach (var entry in ChangeTracker.Entries<AuditableEntity>())
        {
            if (entry.State == EntityState.Added)
            {
                entry.Entity.SetCreatedAudit(occurredAtUtc, currentUser.UserId);
            }
            else if (entry.State == EntityState.Modified)
            {
                entry.Entity.SetModifiedAudit(occurredAtUtc, currentUser.UserId);
            }
        }
    }
}

using Microsoft.EntityFrameworkCore;
using SiteInspect.Application.Features.Inspections.Common;
using SiteInspect.Domain.Inspections.InspectionAggregate;

namespace SiteInspect.Infrastructure.Persistence.Repositories;

internal sealed class InspectionRepository(ApplicationDbContext dbContext) : IInspectionRepository
{
    // Force a root write even when adding another action leaves the status unchanged,
    // so EF's concurrency check still covers every creation.
    public void Update(Inspection inspection) =>
        dbContext.Entry(inspection).State = EntityState.Modified;

    public Task<Inspection?> GetForUpdateAsync(
        Guid inspectionId,
        CancellationToken cancellationToken = default) =>
        dbContext.Inspections
            .SingleOrDefaultAsync(item => item.Id == inspectionId, cancellationToken);

    public Task AddAsync(Inspection inspection, CancellationToken cancellationToken = default) =>
        dbContext.Inspections.AddAsync(inspection, cancellationToken).AsTask();

    public Task<Inspection?> GetWithObservationsForUpdateAsync(
        Guid inspectionId,
        CancellationToken cancellationToken = default) =>
        dbContext.Inspections
            .AsSplitQuery()
            .Include(item => item.Observations)
            .ThenInclude(item => item.Attachments)
            .SingleOrDefaultAsync(item => item.Id == inspectionId, cancellationToken);

    public Task<Inspection?> GetWithAttachmentsAsync(
        Guid inspectionId,
        CancellationToken cancellationToken = default) =>
        dbContext.Inspections
            .AsNoTracking()
            .AsSplitQuery()
            .Include(item => item.Observations)
            .ThenInclude(item => item.Attachments)
            .SingleOrDefaultAsync(item => item.Id == inspectionId, cancellationToken);
}

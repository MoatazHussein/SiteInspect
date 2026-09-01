using Microsoft.EntityFrameworkCore;
using SiteInspect.Application.Features.Inspections.Common;
using SiteInspect.Domain.Inspections.InspectionAggregate;

namespace SiteInspect.Infrastructure.Persistence.Repositories;

internal sealed class InspectionRepository(ApplicationDbContext dbContext) : IInspectionRepository
{
    public async Task<Inspection?> GetForUpdateAsync(
        Guid inspectionId,
        byte[] expectedRowVersion,
        CancellationToken cancellationToken = default)
    {
        var inspection = await dbContext.Inspections
            .SingleOrDefaultAsync(item => item.Id == inspectionId, cancellationToken);

        if (inspection is not null)
        {
            dbContext.Entry(inspection)
                .Property(item => item.RowVersion)
                .OriginalValue = expectedRowVersion;
        }

        return inspection;
    }

    public Task AddAsync(Inspection inspection, CancellationToken cancellationToken = default) =>
        dbContext.Inspections.AddAsync(inspection, cancellationToken).AsTask();

    public async Task<Inspection?> GetWithObservationsForUpdateAsync(
        Guid inspectionId,
        byte[] expectedRowVersion,
        CancellationToken cancellationToken = default)
    {
        var inspection = await dbContext.Inspections
            .AsSplitQuery()
            .Include(item => item.Observations)
            .ThenInclude(item => item.Attachments)
            .SingleOrDefaultAsync(item => item.Id == inspectionId, cancellationToken);

        if (inspection is not null)
        {
            dbContext.Entry(inspection)
                .Property(item => item.RowVersion)
                .OriginalValue = expectedRowVersion;
        }

        return inspection;
    }

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

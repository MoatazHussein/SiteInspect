using SiteInspect.Domain.Inspections.InspectionAggregate;

namespace SiteInspect.Application.Features.Inspections.Common;

public interface IInspectionRepository
{
    void Update(Inspection inspection);

    Task<Inspection?> GetForUpdateAsync(
        Guid inspectionId,
        CancellationToken cancellationToken = default);

    Task<Inspection?> GetWithObservationsForUpdateAsync(
        Guid inspectionId,
        CancellationToken cancellationToken = default);

    Task<Inspection?> GetWithAttachmentsAsync(
        Guid inspectionId,
        CancellationToken cancellationToken = default);

    Task AddAsync(Inspection inspection, CancellationToken cancellationToken = default);
}

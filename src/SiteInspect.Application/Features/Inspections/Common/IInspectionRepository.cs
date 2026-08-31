using SiteInspect.Domain.Inspections.InspectionAggregate;

namespace SiteInspect.Application.Features.Inspections.Common;

public interface IInspectionRepository
{
    Task<Inspection?> GetForUpdateAsync(
        Guid inspectionId,
        byte[] expectedRowVersion,
        CancellationToken cancellationToken = default);

    Task<Inspection?> GetWithObservationsForUpdateAsync(
        Guid inspectionId,
        byte[] expectedRowVersion,
        CancellationToken cancellationToken = default);

    Task AddAsync(Inspection inspection, CancellationToken cancellationToken = default);
}

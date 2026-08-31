using SiteInspect.Application.Common.Results;
using SiteInspect.Application.Features.Inspections.Queries.GetInspection;
using SiteInspect.Application.Features.Inspections.Queries.GetInspectionFilterOptions;
using SiteInspect.Application.Features.Inspections.Queries.GetInspections;

namespace SiteInspect.Application.Features.Inspections.Common;

public interface IInspectionReadService
{
    Task<Result<GetInspectionsResponse>> ListAsync(
        GetInspectionsQuery query,
        CancellationToken cancellationToken = default);

    Task<Result<GetInspectionResponse>> GetAsync(
        Guid inspectionId,
        CancellationToken cancellationToken = default);

    Task<Result<GetInspectionFilterOptionsResponse>> GetFilterOptionsAsync(
        CancellationToken cancellationToken = default);
}

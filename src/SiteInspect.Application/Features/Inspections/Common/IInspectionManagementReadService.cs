using SiteInspect.Application.Common.Results;
using SiteInspect.Application.Features.Inspections.Queries.GetInspectionManagementOptions;

namespace SiteInspect.Application.Features.Inspections.Common;

public interface IInspectionManagementReadService
{
    Task<Result<GetInspectionManagementOptionsResponse>> GetOptionsAsync(
        CancellationToken cancellationToken = default);
}

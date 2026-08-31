using MediatR;
using SiteInspect.Application.Common.Results;
using SiteInspect.Application.Features.Inspections.Common;

namespace SiteInspect.Application.Features.Inspections.Queries.GetInspectionFilterOptions;

public sealed class GetInspectionFilterOptionsHandler(IInspectionReadService readService)
    : IRequestHandler<GetInspectionFilterOptionsQuery, Result<GetInspectionFilterOptionsResponse>>
{
    public Task<Result<GetInspectionFilterOptionsResponse>> Handle(
        GetInspectionFilterOptionsQuery query,
        CancellationToken cancellationToken) =>
        readService.GetFilterOptionsAsync(cancellationToken);
}

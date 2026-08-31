using MediatR;
using SiteInspect.Application.Common.Results;
using SiteInspect.Application.Features.Inspections.Common;

namespace SiteInspect.Application.Features.Inspections.Queries.GetInspection;

public sealed class GetInspectionHandler(IInspectionReadService readService)
    : IRequestHandler<GetInspectionQuery, Result<GetInspectionResponse>>
{
    public Task<Result<GetInspectionResponse>> Handle(
        GetInspectionQuery query,
        CancellationToken cancellationToken) =>
        readService.GetAsync(query.InspectionId, cancellationToken);
}

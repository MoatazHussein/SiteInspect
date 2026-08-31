using MediatR;
using SiteInspect.Application.Common.Results;
using SiteInspect.Application.Features.Inspections.Common;

namespace SiteInspect.Application.Features.Inspections.Queries.GetInspections;

public sealed class GetInspectionsHandler(IInspectionReadService readService)
    : IRequestHandler<GetInspectionsQuery, Result<GetInspectionsResponse>>
{
    public Task<Result<GetInspectionsResponse>> Handle(
        GetInspectionsQuery query,
        CancellationToken cancellationToken) =>
        readService.ListAsync(query, cancellationToken);
}

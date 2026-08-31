using MediatR;
using SiteInspect.Application.Common.Abstractions.Identity;
using SiteInspect.Application.Common.Authorization;
using SiteInspect.Application.Common.Errors;
using SiteInspect.Application.Common.Results;
using SiteInspect.Application.Features.Inspections.Common;

namespace SiteInspect.Application.Features.Inspections.Queries.GetInspectionManagementOptions;

public sealed class GetInspectionManagementOptionsHandler(
    ICurrentUser currentUser,
    IInspectionManagementReadService readService)
    : IRequestHandler<GetInspectionManagementOptionsQuery, Result<GetInspectionManagementOptionsResponse>>
{
    public Task<Result<GetInspectionManagementOptionsResponse>> Handle(
        GetInspectionManagementOptionsQuery query,
        CancellationToken cancellationToken) =>
        currentUser.IsInRole(RoleNames.Manager)
            ? readService.GetOptionsAsync(cancellationToken)
            : Task.FromResult(Result<GetInspectionManagementOptionsResponse>.Failure(AuthorizationErrors.Forbidden));
}

using MediatR;
using SiteInspect.Application.Common.Abstractions.Identity;
using SiteInspect.Application.Common.Authorization;
using SiteInspect.Application.Common.Errors;
using SiteInspect.Application.Common.Results;
using SiteInspect.Application.Features.Dashboard.Common;

namespace SiteInspect.Application.Features.Dashboard.Queries.GetDashboardSummary;

public sealed class GetDashboardSummaryHandler(ICurrentUser currentUser, IDashboardReadService readService)
    : IRequestHandler<GetDashboardSummaryQuery, Result<DashboardSummaryResponse>>
{
    public async Task<Result<DashboardSummaryResponse>> Handle(
        GetDashboardSummaryQuery query, CancellationToken cancellationToken)
    {
        if (!currentUser.IsInRole(RoleNames.Manager))
        {
            return Result<DashboardSummaryResponse>.Failure(AuthorizationErrors.Forbidden);
        }

        return Result<DashboardSummaryResponse>.Success(await readService.GetSummaryAsync(query, cancellationToken));
    }
}

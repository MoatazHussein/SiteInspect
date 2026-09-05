using SiteInspect.Application.Features.Dashboard.Queries.GetDashboardSummary;

namespace SiteInspect.Application.Features.Dashboard.Common;

public interface IDashboardReadService
{
    Task<DashboardSummaryResponse> GetSummaryAsync(GetDashboardSummaryQuery query, CancellationToken cancellationToken);
}

using SiteInspect.Application.Common.Messaging;
using SiteInspect.Application.Features.Dashboard.Common;
using SiteInspect.Domain.Inspections.InspectionAggregate;

namespace SiteInspect.Application.Features.Dashboard.Queries.GetDashboardSummary;

public sealed record GetDashboardSummaryQuery(
    Guid? ProjectId = null,
    InspectionStatus? Status = null,
    DateTimeOffset? DueFromUtc = null,
    DateTimeOffset? DueBeforeUtc = null) : IQuery<DashboardSummaryResponse>;

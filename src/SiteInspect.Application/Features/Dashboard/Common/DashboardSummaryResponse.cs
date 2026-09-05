namespace SiteInspect.Application.Features.Dashboard.Common;

public sealed record DashboardSummaryResponse(
    int ActiveInspections,
    int OverdueInspections,
    int CompletedInspections,
    int OutstandingCorrectiveActions,
    int ActionsAwaitingReview,
    int OverdueCorrectiveActions,
    DateTimeOffset GeneratedAtUtc,
    IReadOnlyCollection<DashboardInspectionItem> OverdueInspectionItems,
    IReadOnlyCollection<DashboardActionItem> OutstandingActionItems);

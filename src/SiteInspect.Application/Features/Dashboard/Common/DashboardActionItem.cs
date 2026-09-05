using SiteInspect.Domain.CorrectiveActions.CorrectiveActionAggregate;

namespace SiteInspect.Application.Features.Dashboard.Common;

public sealed record DashboardActionItem(
    Guid Id, Guid InspectionId, string InspectionNumber, string ProjectName,
    string ContractorName, string Description, CorrectiveActionStatus Status, DateTimeOffset DueAtUtc);

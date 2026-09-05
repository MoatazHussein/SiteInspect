using SiteInspect.Domain.Inspections.InspectionAggregate;

namespace SiteInspect.Application.Features.Dashboard.Common;

public sealed record DashboardInspectionItem(
    Guid Id, string Number, string ProjectName, string InspectorName,
    InspectionStatus Status, DateTimeOffset DueAtUtc);

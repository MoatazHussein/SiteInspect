using SiteInspect.Domain.Inspections.InspectionAggregate;

namespace SiteInspect.Application.Features.Inspections.Queries.GetInspections;

public sealed record InspectionListItem(
    Guid Id,
    string Number,
    string ProjectName,
    string LocationName,
    string TemplateName,
    int TemplateVersion,
    Guid AssignedInspectorId,
    string AssignedInspectorName,
    InspectionStatus Status,
    DateTimeOffset DueAtUtc,
    string RowVersion);

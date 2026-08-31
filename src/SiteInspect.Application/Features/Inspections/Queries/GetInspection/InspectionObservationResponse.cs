using SiteInspect.Domain.Inspections;
using SiteInspect.Domain.Inspections.InspectionAggregate;

namespace SiteInspect.Application.Features.Inspections.Queries.GetInspection;

public sealed record InspectionObservationResponse(
    Guid Id,
    string SectionName,
    string Question,
    int DisplayOrder,
    bool IsRequired,
    ObservationOutcome? Outcome,
    Severity Severity,
    string? Notes,
    DateTimeOffset? ObservedAtUtc);

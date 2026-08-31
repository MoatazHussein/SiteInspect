namespace SiteInspect.Domain.Inspections.InspectionAggregate;

public sealed record InspectionObservationDraft(
    Guid ObservationId,
    ObservationOutcome? Outcome,
    string? Notes);

using SiteInspect.Domain.Inspections.InspectionAggregate;

namespace SiteInspect.Application.Features.Inspections.Commands.SaveInspectionDraft;

public sealed record SaveInspectionObservationDraft(
    Guid ObservationId,
    ObservationOutcome? Outcome,
    string? Notes);

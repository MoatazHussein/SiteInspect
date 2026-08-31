using SiteInspect.Domain.Inspections.InspectionAggregate;

namespace SiteInspect.Application.Features.Inspections.Queries.GetInspection;

public sealed record GetInspectionResponse(
    Guid Id,
    string Number,
    Guid ProjectId,
    string ProjectName,
    Guid LocationId,
    string LocationName,
    Guid TemplateId,
    string TemplateName,
    int TemplateVersion,
    Guid AssignedInspectorId,
    string AssignedInspectorName,
    InspectionStatus Status,
    DateTimeOffset DueAtUtc,
    DateTimeOffset? StartedAtUtc,
    DateTimeOffset? SubmittedAtUtc,
    DateTimeOffset? CompletedAtUtc,
    DateTimeOffset? CancelledAtUtc,
    string? CancellationReason,
    DateTimeOffset? LastDraftSavedAtUtc,
    string RowVersion,
    IReadOnlyCollection<InspectionObservationResponse> Observations);

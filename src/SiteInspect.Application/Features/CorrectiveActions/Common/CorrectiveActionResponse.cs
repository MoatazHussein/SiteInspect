using SiteInspect.Domain.CorrectiveActions.CorrectiveActionAggregate;

namespace SiteInspect.Application.Features.CorrectiveActions.Common;

public sealed record CorrectiveActionResponse(
    Guid Id,
    Guid InspectionId,
    Guid ObservationId,
    Guid AssignedContractorId,
    string AssignedContractorName,
    string Description,
    DateTimeOffset DueAtUtc,
    CorrectiveActionStatus Status,
    string? ResolutionNotes,
    string? RejectionReason,
    DateTimeOffset? SubmittedAtUtc,
    DateTimeOffset? RejectedAtUtc,
    DateTimeOffset? ClosedAtUtc,
    DateTimeOffset CreatedAtUtc,
    string RowVersion);


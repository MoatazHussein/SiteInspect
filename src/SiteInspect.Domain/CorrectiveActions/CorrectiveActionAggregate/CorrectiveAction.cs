using SiteInspect.Domain.Common.Entities;
using SiteInspect.Domain.Inspections.InspectionAggregate;

namespace SiteInspect.Domain.CorrectiveActions.CorrectiveActionAggregate;

public sealed class CorrectiveAction : ConcurrentAuditableEntity
{
    private CorrectiveAction() { }

    public Guid InspectionId { get; private set; }
    public Guid ObservationId { get; private set; }
    public Guid AssignedContractorId { get; private set; }
    public string Description { get; private set; } = string.Empty;
    public DateTimeOffset DueAtUtc { get; private set; }
    public CorrectiveActionStatus Status { get; private set; }
    public string? ResolutionNotes { get; private set; }
    public string? RejectionReason { get; private set; }
    public DateTimeOffset? SubmittedAtUtc { get; private set; }
    public DateTimeOffset? RejectedAtUtc { get; private set; }
    public DateTimeOffset? ClosedAtUtc { get; private set; }

    public static CorrectiveAction Create(
        Inspection inspection,
        Guid observationId,
        Guid contractorId,
        string description,
        DateTimeOffset dueAtUtc,
        DateTimeOffset now)
    {
        ArgumentNullException.ThrowIfNull(inspection);
        ArgumentException.ThrowIfNullOrWhiteSpace(description);
        if (contractorId == Guid.Empty)
        {
            throw new ArgumentException("A contractor is required.", nameof(contractorId));
        }
        if (description.Trim().Length > 2000)
        {
            throw new ArgumentException("The description cannot exceed 2000 characters.", nameof(description));
        }
        if (dueAtUtc <= now)
        {
            throw new ArgumentException("The due date must be in the future.", nameof(dueAtUtc));
        }

        inspection.OpenCorrectiveActions(observationId);
        return new CorrectiveAction
        {
            InspectionId = inspection.Id,
            ObservationId = observationId,
            AssignedContractorId = contractorId,
            Description = description.Trim(),
            DueAtUtc = dueAtUtc.ToUniversalTime(),
            Status = CorrectiveActionStatus.Open,
        };
    }

    public void SubmitResponse(Guid contractorId, string notes, DateTimeOffset submittedAtUtc)
    {
        if (contractorId != AssignedContractorId)
        {
            throw new InvalidOperationException("Only the assigned contractor may respond.");
        }
        EnsureStatus(CorrectiveActionStatus.Open);
        ArgumentException.ThrowIfNullOrWhiteSpace(notes);
        if (notes.Trim().Length > 2000)
        {
            throw new ArgumentException("Resolution notes cannot exceed 2000 characters.", nameof(notes));
        }

        ResolutionNotes = notes.Trim();
        SubmittedAtUtc = submittedAtUtc;
        Status = CorrectiveActionStatus.ReadyForReview;
    }

    public void Reject(string reason, DateTimeOffset rejectedAtUtc)
    {
        EnsureStatus(CorrectiveActionStatus.ReadyForReview);
        ArgumentException.ThrowIfNullOrWhiteSpace(reason);
        if (reason.Trim().Length > 2000)
        {
            throw new ArgumentException("The rejection reason cannot exceed 2000 characters.", nameof(reason));
        }

        RejectionReason = reason.Trim();
        RejectedAtUtc = rejectedAtUtc;
        Status = CorrectiveActionStatus.Open;
    }

    public void Close(DateTimeOffset closedAtUtc)
    {
        EnsureStatus(CorrectiveActionStatus.ReadyForReview);
        ClosedAtUtc = closedAtUtc;
        Status = CorrectiveActionStatus.Closed;
    }

    private void EnsureStatus(CorrectiveActionStatus expected)
    {
        if (Status != expected)
        {
            throw new InvalidOperationException($"This operation requires status {expected}.");
        }
    }
}


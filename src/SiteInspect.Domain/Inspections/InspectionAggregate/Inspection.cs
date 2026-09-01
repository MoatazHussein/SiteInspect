using SiteInspect.Domain.Common.Entities;
using SiteInspect.Domain.Inspections;
using SiteInspect.Domain.Inspections.InspectionTemplateAggregate;

namespace SiteInspect.Domain.Inspections.InspectionAggregate;

public sealed class Inspection : ConcurrentAuditableEntity
{
    private readonly List<InspectionObservation> observations = [];

    private Inspection()
    {
    }

    internal Inspection(
        Guid id,
        string number,
        Guid projectId,
        Guid projectLocationId,
        Guid templateId,
        int templateVersion,
        Guid assignedInspectorId,
        InspectionStatus status,
        DateTimeOffset dueAtUtc,
        DateTimeOffset? startedAtUtc,
        DateTimeOffset? submittedAtUtc,
        DateTimeOffset? completedAtUtc,
        DateTimeOffset? cancelledAtUtc,
        string? cancellationReason,
        DateTimeOffset? lastDraftSavedAtUtc,
        IEnumerable<InspectionObservation> inspectionObservations)
    {
        Id = id;
        Number = number;
        ProjectId = projectId;
        ProjectLocationId = projectLocationId;
        TemplateId = templateId;
        TemplateVersion = templateVersion;
        AssignedInspectorId = assignedInspectorId;
        Status = status;
        DueAtUtc = dueAtUtc;
        StartedAtUtc = startedAtUtc;
        SubmittedAtUtc = submittedAtUtc;
        CompletedAtUtc = completedAtUtc;
        CancelledAtUtc = cancelledAtUtc;
        CancellationReason = cancellationReason;
        LastDraftSavedAtUtc = lastDraftSavedAtUtc;
        observations.AddRange(inspectionObservations);
    }

    public static Inspection Create(
        string number,
        Guid projectId,
        Guid projectLocationId,
        InspectionTemplate template,
        Guid assignedInspectorId,
        DateTimeOffset dueAtUtc)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(number);
        ArgumentNullException.ThrowIfNull(template);

        if (!template.IsActive)
        {
            throw new InvalidOperationException("An inspection can only be created from an active template.");
        }

        if (template.Items.Count == 0)
        {
            throw new InvalidOperationException("An inspection template must contain at least one item.");
        }

        var inspectionId = Guid.CreateVersion7();
        var inspectionObservations = template.Items.Select(item => new InspectionObservation(
            Guid.CreateVersion7(),
            inspectionId,
            item.Id,
            item.SectionName,
            item.Question,
            item.DisplayOrder,
            item.IsRequired,
            null,
            item.DefaultSeverity,
            null,
            null,
            [])).ToArray();

        return new Inspection(
            inspectionId,
            number.Trim(),
            projectId,
            projectLocationId,
            template.Id,
            template.Version,
            assignedInspectorId,
            InspectionStatus.Assigned,
            dueAtUtc,
            null,
            null,
            null,
            null,
            null,
            null,
            inspectionObservations);
    }

    public string Number { get; private set; } = string.Empty;

    public Guid ProjectId { get; private set; }

    public Guid ProjectLocationId { get; private set; }

    public Guid TemplateId { get; private set; }

    public int TemplateVersion { get; private set; }

    public Guid AssignedInspectorId { get; private set; }

    public InspectionStatus Status { get; private set; }

    public DateTimeOffset DueAtUtc { get; private set; }

    public DateTimeOffset? StartedAtUtc { get; private set; }

    public DateTimeOffset? SubmittedAtUtc { get; private set; }

    public DateTimeOffset? CompletedAtUtc { get; private set; }

    public DateTimeOffset? CancelledAtUtc { get; private set; }

    public string? CancellationReason { get; private set; }

    public DateTimeOffset? LastDraftSavedAtUtc { get; private set; }

    public IReadOnlyCollection<InspectionObservation> Observations => observations;

    public void Start(Guid inspectorId, DateTimeOffset startedAtUtc)
    {
        EnsureAssignedInspector(inspectorId);

        if (Status == InspectionStatus.InProgress)
        {
            return;
        }

        if (Status != InspectionStatus.Assigned)
        {
            throw new InvalidOperationException("Only an assigned inspection can be started.");
        }

        Status = InspectionStatus.InProgress;
        StartedAtUtc = startedAtUtc;
    }

    public void SaveDraft(
        Guid inspectorId,
        IReadOnlyCollection<InspectionObservationDraft> drafts,
        DateTimeOffset observedAtUtc)
    {
        EnsureAssignedInspector(inspectorId);

        if (Status != InspectionStatus.InProgress)
        {
            throw new InvalidOperationException("Only an in-progress inspection can be edited.");
        }

        foreach (var draft in drafts)
        {
            var observation = observations.SingleOrDefault(item => item.Id == draft.ObservationId)
                ?? throw new InvalidOperationException(
                    $"Observation '{draft.ObservationId}' does not belong to this inspection.");

            observation.RecordDraft(draft.Outcome, draft.Notes, observedAtUtc);
        }

        LastDraftSavedAtUtc = observedAtUtc;
    }

    public InspectionAttachment AddAttachment(
        Guid inspectorId,
        Guid observationId,
        string originalFileName,
        string storedFileName,
        string contentType,
        long length,
        DateTimeOffset addedAtUtc)
    {
        EnsureAssignedInspector(inspectorId);

        if (Status != InspectionStatus.InProgress)
        {
            throw new InvalidOperationException("Attachments can only be added to an in-progress inspection.");
        }

        var observation = observations.SingleOrDefault(item => item.Id == observationId)
            ?? throw new InvalidOperationException(
                $"Observation '{observationId}' does not belong to this inspection.");

        var attachment = observation.AddAttachment(
            originalFileName,
            storedFileName,
            contentType,
            length);

        LastDraftSavedAtUtc = addedAtUtc;
        return attachment;
    }

    public void Submit(Guid inspectorId, DateTimeOffset submittedAtUtc)
    {
        EnsureAssignedInspector(inspectorId);

        if (Status != InspectionStatus.InProgress)
        {
            throw new InvalidOperationException("Only an in-progress inspection can be submitted.");
        }

        if (observations.Any(item => item.IsRequiredSnapshot && item.Outcome is null))
        {
            throw new InvalidOperationException("Every required observation must have an outcome.");
        }

        if (observations.Any(item =>
            item.Outcome == ObservationOutcome.Fail && string.IsNullOrWhiteSpace(item.Notes)))
        {
            throw new InvalidOperationException("Every failed observation must include notes.");
        }

        if (observations.Any(item =>
            item.Outcome == ObservationOutcome.Fail &&
            item.Severity is Severity.High or Severity.Critical &&
            item.Attachments.Count == 0))
        {
            throw new InvalidOperationException(
                "Every High or Critical failed observation must include a photo.");
        }

        Status = InspectionStatus.Submitted;
        SubmittedAtUtc = submittedAtUtc;
    }

    public void Reassign(Guid inspectorId)
    {
        if (Status != InspectionStatus.Assigned)
        {
            throw new InvalidOperationException("Only an assigned inspection can be reassigned.");
        }

        if (inspectorId == Guid.Empty)
        {
            throw new ArgumentException("An inspector is required.", nameof(inspectorId));
        }

        AssignedInspectorId = inspectorId;
    }

    public void Cancel(string reason, DateTimeOffset cancelledAtUtc)
    {
        if (Status is not (InspectionStatus.Assigned or InspectionStatus.InProgress))
        {
            throw new InvalidOperationException("Only an assigned or in-progress inspection can be cancelled.");
        }

        ArgumentException.ThrowIfNullOrWhiteSpace(reason);

        Status = InspectionStatus.Cancelled;
        CancelledAtUtc = cancelledAtUtc;
        CancellationReason = reason.Trim();
    }

    private void EnsureAssignedInspector(Guid inspectorId)
    {
        if (AssignedInspectorId != inspectorId)
        {
            throw new InvalidOperationException("Only the assigned inspector can modify this inspection.");
        }
    }
}

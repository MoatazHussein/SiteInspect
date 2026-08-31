using SiteInspect.Domain.Common.Entities;
using SiteInspect.Domain.Inspections;

namespace SiteInspect.Domain.Inspections.InspectionAggregate;

public sealed class InspectionObservation : BaseEntity
{
    private readonly List<InspectionAttachment> attachments = [];

    private InspectionObservation()
    {
    }

    internal InspectionObservation(
        Guid id,
        Guid inspectionId,
        Guid templateItemId,
        string sectionNameSnapshot,
        string questionSnapshot,
        int displayOrderSnapshot,
        bool isRequiredSnapshot,
        ObservationOutcome? outcome,
        Severity severity,
        string? notes,
        DateTimeOffset? observedAtUtc,
        IEnumerable<InspectionAttachment> inspectionAttachments)
    {
        Id = id;
        InspectionId = inspectionId;
        TemplateItemId = templateItemId;
        SectionNameSnapshot = sectionNameSnapshot;
        QuestionSnapshot = questionSnapshot;
        DisplayOrderSnapshot = displayOrderSnapshot;
        IsRequiredSnapshot = isRequiredSnapshot;
        Outcome = outcome;
        Severity = severity;
        Notes = notes;
        ObservedAtUtc = observedAtUtc;
        attachments.AddRange(inspectionAttachments);
    }

    public Guid InspectionId { get; private set; }

    public Guid TemplateItemId { get; private set; }

    public string SectionNameSnapshot { get; private set; } = string.Empty;

    public string QuestionSnapshot { get; private set; } = string.Empty;

    public int DisplayOrderSnapshot { get; private set; }

    public bool IsRequiredSnapshot { get; private set; }

    public ObservationOutcome? Outcome { get; private set; }

    public Severity Severity { get; private set; }

    public string? Notes { get; private set; }

    public DateTimeOffset? ObservedAtUtc { get; private set; }

    public IReadOnlyCollection<InspectionAttachment> Attachments => attachments;

    internal void RecordDraft(
        ObservationOutcome? outcome,
        string? notes,
        DateTimeOffset observedAtUtc)
    {
        Outcome = outcome;
        Notes = string.IsNullOrWhiteSpace(notes) ? null : notes.Trim();
        ObservedAtUtc = outcome is null && Notes is null ? null : observedAtUtc;
    }
}

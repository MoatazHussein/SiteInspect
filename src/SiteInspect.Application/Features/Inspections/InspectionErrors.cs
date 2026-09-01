using SiteInspect.Application.Common.Results;

namespace SiteInspect.Application.Features.Inspections;

public static class InspectionErrors
{
    public static readonly Error InvalidPaging = new(
        "Inspection.InvalidPaging",
        "Page must be positive and page size must be between 1 and 100.",
        ErrorType.Validation);

    public static Error NotFound(Guid inspectionId) => new(
        "Inspection.NotFound",
        $"Inspection '{inspectionId}' was not found.",
        ErrorType.NotFound);

    public static readonly Error ProjectNotFound = new(
        "Inspection.ProjectNotFound",
        "The selected active project was not found.",
        ErrorType.Validation);

    public static readonly Error LocationNotFound = new(
        "Inspection.LocationNotFound",
        "The selected location does not belong to the selected project.",
        ErrorType.Validation);

    public static readonly Error TemplateNotFound = new(
        "Inspection.TemplateNotFound",
        "The selected active inspection template was not found.",
        ErrorType.Validation);

    public static readonly Error InspectorNotFound = new(
        "Inspection.InspectorNotFound",
        "The selected inspector was not found.",
        ErrorType.Validation);

    public static readonly Error ReassignmentNotAllowed = new(
        "Inspection.ReassignmentNotAllowed",
        "Only an assigned inspection can be reassigned.",
        ErrorType.Conflict);

    public static readonly Error CancellationNotAllowed = new(
        "Inspection.CancellationNotAllowed",
        "Only an assigned or in-progress inspection can be cancelled.",
        ErrorType.Conflict);

    public static readonly Error AssignedInspectorRequired = new(
        "Inspection.AssignedInspectorRequired",
        "Only the assigned inspector can perform this operation.",
        ErrorType.Forbidden);

    public static readonly Error StartNotAllowed = new(
        "Inspection.StartNotAllowed",
        "Only an assigned inspection can be started.",
        ErrorType.Conflict);

    public static readonly Error DraftNotAllowed = new(
        "Inspection.DraftNotAllowed",
        "Only an in-progress inspection can be edited.",
        ErrorType.Conflict);

    public static Error ObservationNotFound(Guid observationId) => new(
        "Inspection.ObservationNotFound",
        $"Observation '{observationId}' does not belong to this inspection.",
        ErrorType.Validation);

    public static readonly Error AttachmentNotAllowed = new(
        "Inspection.AttachmentNotAllowed",
        "Attachments can only be added to an in-progress inspection by its assigned inspector.",
        ErrorType.Conflict);

    public static readonly Error AttachmentLimitReached = new(
        "Inspection.AttachmentLimitReached",
        "An observation cannot have more than five attachments.",
        ErrorType.Validation);

    public static readonly Error InvalidAttachmentContent = new(
        "Inspection.InvalidAttachmentContent",
        "The uploaded file content does not match a supported image format.",
        ErrorType.Validation);

    public static Error AttachmentNotFound(Guid attachmentId) => new(
        "Inspection.AttachmentNotFound",
        $"Attachment '{attachmentId}' was not found.",
        ErrorType.NotFound);

    public static readonly Error SubmissionNotAllowed = new(
        "Inspection.SubmissionNotAllowed",
        "Only an in-progress inspection can be submitted.",
        ErrorType.Conflict);

    public static readonly Error RequiredObservationsIncomplete = new(
        "Inspection.RequiredObservationsIncomplete",
        "Every required observation must have an outcome before submission.",
        ErrorType.Validation);

    public static readonly Error FailedObservationNotesRequired = new(
        "Inspection.FailedObservationNotesRequired",
        "Every failed observation must include notes before submission.",
        ErrorType.Validation);

    public static readonly Error FailurePhotoRequired = new(
        "Inspection.FailurePhotoRequired",
        "Every High or Critical failed observation must include a photo before submission.",
        ErrorType.Validation);
}

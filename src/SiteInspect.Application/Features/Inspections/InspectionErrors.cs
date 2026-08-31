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
}

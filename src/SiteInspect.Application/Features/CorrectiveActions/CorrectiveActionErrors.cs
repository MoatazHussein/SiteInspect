using SiteInspect.Application.Common.Results;

namespace SiteInspect.Application.Features.CorrectiveActions;

public static class CorrectiveActionErrors
{
    public static Error NotFound(Guid correctiveActionId) => new(
        "CorrectiveAction.NotFound",
        $"Corrective action '{correctiveActionId}' was not found.",
        ErrorType.NotFound);

    public static readonly Error CreationNotAllowed = new(
        "CorrectiveAction.CreationNotAllowed", "Actions can only be created after inspection submission.", ErrorType.Conflict);
    public static readonly Error FailedObservationRequired = new(
        "CorrectiveAction.FailedObservationRequired", "Select a failed observation belonging to this inspection.", ErrorType.Validation);
    public static readonly Error AlreadyExists = new(
        "CorrectiveAction.AlreadyExists", "This observation already has a corrective action.", ErrorType.Conflict);
    public static readonly Error ContractorNotFound = new(
        "CorrectiveAction.ContractorNotFound", "The selected contractor was not found.", ErrorType.Validation);

    public static readonly Error AssignedContractorRequired = new(
        "CorrectiveAction.AssignedContractorRequired",
        "Only the assigned contractor can respond to this corrective action.",
        ErrorType.Forbidden);

    public static readonly Error ResponseNotAllowed = new(
        "CorrectiveAction.ResponseNotAllowed",
        "Only an open corrective action can be submitted for review.",
        ErrorType.Conflict);

    public static readonly Error ReviewNotAllowed = new(
        "CorrectiveAction.ReviewNotAllowed",
        "Only a corrective action ready for review can be approved or rejected.",
        ErrorType.Conflict);
}


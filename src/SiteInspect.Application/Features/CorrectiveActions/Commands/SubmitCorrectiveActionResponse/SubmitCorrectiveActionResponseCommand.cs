using SiteInspect.Application.Common.Messaging;
using SiteInspect.Application.Features.CorrectiveActions.Common;

namespace SiteInspect.Application.Features.CorrectiveActions.Commands.SubmitCorrectiveActionResponse;

public sealed record SubmitCorrectiveActionResponseCommand(
    Guid CorrectiveActionId,
    string ResolutionNotes,
    string RowVersion) : ICommand<CorrectiveActionMutationResponse>;

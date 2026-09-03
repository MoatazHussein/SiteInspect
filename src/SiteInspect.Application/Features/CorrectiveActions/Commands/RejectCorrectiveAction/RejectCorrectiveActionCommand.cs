using SiteInspect.Application.Common.Messaging;
using SiteInspect.Application.Features.CorrectiveActions.Common;

namespace SiteInspect.Application.Features.CorrectiveActions.Commands.RejectCorrectiveAction;

public sealed record RejectCorrectiveActionCommand(
    Guid CorrectiveActionId,
    string Reason,
    string RowVersion) : ICommand<CorrectiveActionMutationResponse>;

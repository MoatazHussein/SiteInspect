using SiteInspect.Application.Common.Messaging;
using SiteInspect.Application.Features.CorrectiveActions.Common;

namespace SiteInspect.Application.Features.CorrectiveActions.Commands.ApproveCorrectiveAction;

public sealed record ApproveCorrectiveActionCommand(
    Guid CorrectiveActionId,
    string RowVersion) : ICommand<CorrectiveActionMutationResponse>;

using SiteInspect.Application.Common.Messaging;
using SiteInspect.Application.Features.CorrectiveActions.Common;

namespace SiteInspect.Application.Features.CorrectiveActions.Queries.GetAssignedCorrectiveActions;

public sealed record GetAssignedCorrectiveActionsQuery
    : IQuery<IReadOnlyCollection<ContractorCorrectiveActionResponse>>;

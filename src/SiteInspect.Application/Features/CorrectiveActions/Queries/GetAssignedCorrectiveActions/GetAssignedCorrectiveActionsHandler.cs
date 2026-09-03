using MediatR;
using SiteInspect.Application.Common.Abstractions.Identity;
using SiteInspect.Application.Common.Authorization;
using SiteInspect.Application.Common.Errors;
using SiteInspect.Application.Common.Results;
using SiteInspect.Application.Features.CorrectiveActions.Common;

namespace SiteInspect.Application.Features.CorrectiveActions.Queries.GetAssignedCorrectiveActions;

public sealed class GetAssignedCorrectiveActionsHandler(
    ICurrentUser currentUser,
    ICorrectiveActionReadService readService)
    : IRequestHandler<GetAssignedCorrectiveActionsQuery, Result<IReadOnlyCollection<ContractorCorrectiveActionResponse>>>
{
    public Task<Result<IReadOnlyCollection<ContractorCorrectiveActionResponse>>> Handle(
        GetAssignedCorrectiveActionsQuery query,
        CancellationToken cancellationToken) =>
        currentUser.IsInRole(RoleNames.Contractor) && currentUser.UserId is Guid contractorId
            ? readService.ListAssignedAsync(contractorId, cancellationToken)
            : Task.FromResult(
                Result<IReadOnlyCollection<ContractorCorrectiveActionResponse>>.Failure(
                    AuthorizationErrors.Forbidden));
}

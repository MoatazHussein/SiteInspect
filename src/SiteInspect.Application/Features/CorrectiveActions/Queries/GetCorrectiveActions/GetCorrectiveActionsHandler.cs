using MediatR;
using SiteInspect.Application.Common.Abstractions.Identity;
using SiteInspect.Application.Common.Authorization;
using SiteInspect.Application.Common.Errors;
using SiteInspect.Application.Common.Results;
using SiteInspect.Application.Features.CorrectiveActions.Common;

namespace SiteInspect.Application.Features.CorrectiveActions.Queries.GetCorrectiveActions;

public sealed class GetCorrectiveActionsHandler(ICurrentUser currentUser, ICorrectiveActionReadService readService)
    : IRequestHandler<GetCorrectiveActionsQuery, Result<IReadOnlyCollection<CorrectiveActionResponse>>>
{
    public Task<Result<IReadOnlyCollection<CorrectiveActionResponse>>> Handle(
        GetCorrectiveActionsQuery query, CancellationToken cancellationToken) =>
        currentUser.IsInRole(RoleNames.Manager)
            ? readService.ListAsync(query.InspectionId, cancellationToken)
            : Task.FromResult(Result<IReadOnlyCollection<CorrectiveActionResponse>>.Failure(AuthorizationErrors.Forbidden));
}


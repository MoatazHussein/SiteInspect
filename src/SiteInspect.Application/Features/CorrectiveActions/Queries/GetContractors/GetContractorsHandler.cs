using MediatR;
using SiteInspect.Application.Common.Abstractions.Identity;
using SiteInspect.Application.Common.Authorization;
using SiteInspect.Application.Common.Errors;
using SiteInspect.Application.Common.Results;
using SiteInspect.Application.Features.CorrectiveActions.Common;

namespace SiteInspect.Application.Features.CorrectiveActions.Queries.GetContractors;

public sealed class GetContractorsHandler(ICurrentUser currentUser, ICorrectiveActionReadService readService)
    : IRequestHandler<GetContractorsQuery, Result<IReadOnlyCollection<ContractorOption>>>
{
    public async Task<Result<IReadOnlyCollection<ContractorOption>>> Handle(
        GetContractorsQuery query, CancellationToken cancellationToken)
    {
        if (!currentUser.IsInRole(RoleNames.Manager))
        {
            return Result<IReadOnlyCollection<ContractorOption>>.Failure(AuthorizationErrors.Forbidden);
        }
        return Result<IReadOnlyCollection<ContractorOption>>.Success(
            await readService.GetContractorsAsync(cancellationToken));
    }
}

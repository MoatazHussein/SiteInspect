using MediatR;
using SiteInspect.Application.Common.Abstractions.Identity;
using SiteInspect.Application.Common.Abstractions.Persistence;
using SiteInspect.Application.Common.Authorization;
using SiteInspect.Application.Common.Errors;
using SiteInspect.Application.Common.Persistence;
using SiteInspect.Application.Common.Results;
using SiteInspect.Application.Features.CorrectiveActions.Common;
using SiteInspect.Domain.CorrectiveActions.CorrectiveActionAggregate;

namespace SiteInspect.Application.Features.CorrectiveActions.Commands.SubmitCorrectiveActionResponse;

public sealed class SubmitCorrectiveActionResponseHandler(
    ICurrentUser currentUser,
    IRepository<CorrectiveAction> repository,
    IUnitOfWork unitOfWork,
    TimeProvider timeProvider)
    : IRequestHandler<SubmitCorrectiveActionResponseCommand, Result<CorrectiveActionMutationResponse>>
{
    public async Task<Result<CorrectiveActionMutationResponse>> Handle(
        SubmitCorrectiveActionResponseCommand command,
        CancellationToken cancellationToken)
    {
        if (!currentUser.IsInRole(RoleNames.Contractor) || currentUser.UserId is not Guid contractorId)
        {
            return Result<CorrectiveActionMutationResponse>.Failure(AuthorizationErrors.Forbidden);
        }

        var action = await repository.GetByIdAsync(command.CorrectiveActionId, cancellationToken);
        if (action is null)
        {
            return Result<CorrectiveActionMutationResponse>.Failure(
                CorrectiveActionErrors.NotFound(command.CorrectiveActionId));
        }

        action.EnsureCurrentVersion(command.RowVersion);

        if (action.AssignedContractorId != contractorId)
        {
            return Result<CorrectiveActionMutationResponse>.Failure(
                CorrectiveActionErrors.AssignedContractorRequired);
        }
        if (action.Status != CorrectiveActionStatus.Open)
        {
            return Result<CorrectiveActionMutationResponse>.Failure(
                CorrectiveActionErrors.ResponseNotAllowed);
        }

        action.SubmitResponse(contractorId, command.ResolutionNotes, timeProvider.GetUtcNow());
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<CorrectiveActionMutationResponse>.Success(new CorrectiveActionMutationResponse(
            action.Id,
            action.Status,
            Convert.ToBase64String(action.RowVersion)));
    }
}

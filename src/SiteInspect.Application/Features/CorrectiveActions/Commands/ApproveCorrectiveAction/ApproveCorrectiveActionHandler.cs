using MediatR;
using SiteInspect.Application.Common.Abstractions.Identity;
using SiteInspect.Application.Common.Abstractions.Persistence;
using SiteInspect.Application.Common.Authorization;
using SiteInspect.Application.Common.Errors;
using SiteInspect.Application.Common.Persistence;
using SiteInspect.Application.Common.Results;
using SiteInspect.Application.Features.CorrectiveActions.Common;
using SiteInspect.Domain.CorrectiveActions.CorrectiveActionAggregate;

namespace SiteInspect.Application.Features.CorrectiveActions.Commands.ApproveCorrectiveAction;

public sealed class ApproveCorrectiveActionHandler(
    ICurrentUser currentUser,
    IRepository<CorrectiveAction> repository,
    IUnitOfWork unitOfWork,
    TimeProvider timeProvider)
    : IRequestHandler<ApproveCorrectiveActionCommand, Result<CorrectiveActionMutationResponse>>
{
    public async Task<Result<CorrectiveActionMutationResponse>> Handle(
        ApproveCorrectiveActionCommand command,
        CancellationToken cancellationToken)
    {
        if (!currentUser.IsInRole(RoleNames.Manager))
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

        if (action.Status != CorrectiveActionStatus.ReadyForReview)
        {
            return Result<CorrectiveActionMutationResponse>.Failure(CorrectiveActionErrors.ReviewNotAllowed);
        }

        action.Close(timeProvider.GetUtcNow());
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<CorrectiveActionMutationResponse>.Success(new CorrectiveActionMutationResponse(
            action.Id,
            action.Status,
            Convert.ToBase64String(action.RowVersion)));
    }
}

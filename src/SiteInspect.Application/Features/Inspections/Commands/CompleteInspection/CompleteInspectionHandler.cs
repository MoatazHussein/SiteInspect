using MediatR;
using SiteInspect.Application.Common.Abstractions.Identity;
using SiteInspect.Application.Common.Abstractions.Persistence;
using SiteInspect.Application.Common.Authorization;
using SiteInspect.Application.Common.Errors;
using SiteInspect.Application.Common.Persistence;
using SiteInspect.Application.Common.Results;
using SiteInspect.Application.Features.Inspections.Common;
using SiteInspect.Domain.CorrectiveActions.CorrectiveActionAggregate;
using SiteInspect.Domain.Inspections;
using SiteInspect.Domain.Inspections.InspectionAggregate;

namespace SiteInspect.Application.Features.Inspections.Commands.CompleteInspection;

public sealed class CompleteInspectionHandler(
    ICurrentUser currentUser,
    IInspectionRepository inspectionRepository,
    IRepository<CorrectiveAction> correctiveActionRepository,
    IUnitOfWork unitOfWork,
    TimeProvider timeProvider)
    : IRequestHandler<CompleteInspectionCommand, Result<InspectionMutationResponse>>
{
    public async Task<Result<InspectionMutationResponse>> Handle(
        CompleteInspectionCommand command,
        CancellationToken cancellationToken)
    {
        if (!currentUser.IsInRole(RoleNames.Manager))
        {
            return Result<InspectionMutationResponse>.Failure(AuthorizationErrors.Forbidden);
        }

        var inspection = await inspectionRepository.GetWithObservationsForUpdateAsync(
            command.InspectionId,
            cancellationToken);
        if (inspection is null)
        {
            return Result<InspectionMutationResponse>.Failure(InspectionErrors.NotFound(command.InspectionId));
        }

        inspection.EnsureCurrentVersion(command.RowVersion);

        if (inspection.Status is not (InspectionStatus.Submitted or InspectionStatus.CorrectiveActionsOpen))
        {
            return Result<InspectionMutationResponse>.Failure(InspectionErrors.CompletionNotAllowed);
        }

        var actions = await correctiveActionRepository.ListAsync(
            item => item.InspectionId == command.InspectionId,
            cancellationToken);
        var closedObservationIds = actions
            .Where(item => item.Status == CorrectiveActionStatus.Closed)
            .Select(item => item.ObservationId)
            .ToArray();
        var hasIncompleteFailures = inspection.Observations
            .Where(item => item.Outcome == ObservationOutcome.Fail)
            .Any(item => !closedObservationIds.Contains(item.Id));
        if (hasIncompleteFailures)
        {
            return Result<InspectionMutationResponse>.Failure(InspectionErrors.CorrectiveActionsIncomplete);
        }

        inspection.Complete(closedObservationIds, timeProvider.GetUtcNow());
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<InspectionMutationResponse>.Success(new InspectionMutationResponse(
            inspection.Id,
            inspection.Status,
            inspection.AssignedInspectorId,
            Convert.ToBase64String(inspection.RowVersion)));
    }
}

using MediatR;
using SiteInspect.Application.Common.Abstractions.Identity;
using SiteInspect.Application.Common.Abstractions.Persistence;
using SiteInspect.Application.Common.Authorization;
using SiteInspect.Application.Common.Errors;
using SiteInspect.Application.Common.Persistence;
using SiteInspect.Application.Common.Results;
using SiteInspect.Application.Features.CorrectiveActions.Common;
using SiteInspect.Application.Features.Inspections;
using SiteInspect.Application.Features.Inspections.Common;
using SiteInspect.Domain.CorrectiveActions.CorrectiveActionAggregate;
using SiteInspect.Domain.Inspections;
using SiteInspect.Domain.Inspections.InspectionAggregate;

namespace SiteInspect.Application.Features.CorrectiveActions.Commands.CreateCorrectiveAction;

public sealed class CreateCorrectiveActionHandler(
    ICurrentUser currentUser,
    IInspectionRepository inspectionRepository,
    IRepository<CorrectiveAction> actionRepository,
    IContractorDirectory contractorDirectory,
    IUnitOfWork unitOfWork,
    TimeProvider timeProvider)
    : IRequestHandler<CreateCorrectiveActionCommand, Result<CreateCorrectiveActionResponse>>
{
    public async Task<Result<CreateCorrectiveActionResponse>> Handle(
        CreateCorrectiveActionCommand command, CancellationToken cancellationToken)
    {
        if (!currentUser.IsInRole(RoleNames.Manager))
        {
            return Result<CreateCorrectiveActionResponse>.Failure(AuthorizationErrors.Forbidden);
        }

        var inspection = await inspectionRepository.GetWithObservationsForUpdateAsync(
            command.InspectionId, cancellationToken);
        if (inspection is null)
        {
            return Result<CreateCorrectiveActionResponse>.Failure(InspectionErrors.NotFound(command.InspectionId));
        }

        inspection.EnsureCurrentVersion(command.RowVersion);

        if (inspection.Status is not (InspectionStatus.Submitted or InspectionStatus.CorrectiveActionsOpen))
        {
            return Result<CreateCorrectiveActionResponse>.Failure(CorrectiveActionErrors.CreationNotAllowed);
        }
        if (!inspection.Observations.Any(item => item.Id == command.ObservationId && item.Outcome == ObservationOutcome.Fail))
        {
            return Result<CreateCorrectiveActionResponse>.Failure(CorrectiveActionErrors.FailedObservationRequired);
        }
        if (await actionRepository.FirstOrDefaultAsync(
            item => item.ObservationId == command.ObservationId, cancellationToken) is not null)
        {
            return Result<CreateCorrectiveActionResponse>.Failure(CorrectiveActionErrors.AlreadyExists);
        }
        if (!await contractorDirectory.ExistsAsync(command.ContractorId, cancellationToken))
        {
            return Result<CreateCorrectiveActionResponse>.Failure(CorrectiveActionErrors.ContractorNotFound);
        }

        var action = CorrectiveAction.Create(inspection, command.ObservationId,
            command.ContractorId, command.Description, command.DueAtUtc, timeProvider.GetUtcNow());
        await actionRepository.AddAsync(action, cancellationToken);
        inspectionRepository.Update(inspection);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<CreateCorrectiveActionResponse>.Success(new CreateCorrectiveActionResponse(
            action.Id, inspection.Status, Convert.ToBase64String(inspection.RowVersion)));
    }
}


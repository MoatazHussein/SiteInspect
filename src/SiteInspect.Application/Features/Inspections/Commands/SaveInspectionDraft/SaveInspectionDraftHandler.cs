using MediatR;
using SiteInspect.Application.Common.Abstractions.Identity;
using SiteInspect.Application.Common.Abstractions.Persistence;
using SiteInspect.Application.Common.Authorization;
using SiteInspect.Application.Common.Errors;
using SiteInspect.Application.Common.Persistence;
using SiteInspect.Application.Common.Results;
using SiteInspect.Application.Features.Inspections.Common;
using SiteInspect.Domain.Inspections.InspectionAggregate;

namespace SiteInspect.Application.Features.Inspections.Commands.SaveInspectionDraft;

public sealed class SaveInspectionDraftHandler(
    ICurrentUser currentUser,
    IInspectionRepository inspectionRepository,
    IUnitOfWork unitOfWork,
    TimeProvider timeProvider)
    : IRequestHandler<SaveInspectionDraftCommand, Result<InspectionMutationResponse>>
{
    public async Task<Result<InspectionMutationResponse>> Handle(
        SaveInspectionDraftCommand command,
        CancellationToken cancellationToken)
    {
        if (!currentUser.IsInRole(RoleNames.Inspector) || currentUser.UserId is not Guid inspectorId)
        {
            return Result<InspectionMutationResponse>.Failure(AuthorizationErrors.Forbidden);
        }

        var inspection = await inspectionRepository.GetWithObservationsForUpdateAsync(
            command.InspectionId,
            RowVersionToken.Decode(command.RowVersion),
            cancellationToken);

        if (inspection is null)
        {
            return Result<InspectionMutationResponse>.Failure(InspectionErrors.NotFound(command.InspectionId));
        }

        if (inspection.AssignedInspectorId != inspectorId)
        {
            return Result<InspectionMutationResponse>.Failure(InspectionErrors.AssignedInspectorRequired);
        }

        if (inspection.Status != InspectionStatus.InProgress)
        {
            return Result<InspectionMutationResponse>.Failure(InspectionErrors.DraftNotAllowed);
        }

        var observationIds = inspection.Observations.Select(item => item.Id).ToHashSet();
        var missingObservation = command.Observations.FirstOrDefault(
            item => !observationIds.Contains(item.ObservationId));
        if (missingObservation is not null)
        {
            return Result<InspectionMutationResponse>.Failure(
                InspectionErrors.ObservationNotFound(missingObservation.ObservationId));
        }

        var drafts = command.Observations.Select(item => new InspectionObservationDraft(
            item.ObservationId,
            item.Outcome,
            item.Notes)).ToArray();

        inspection.SaveDraft(inspectorId, drafts, timeProvider.GetUtcNow());
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<InspectionMutationResponse>.Success(new InspectionMutationResponse(
            inspection.Id,
            inspection.Status,
            inspection.AssignedInspectorId,
            Convert.ToBase64String(inspection.RowVersion)));
    }
}

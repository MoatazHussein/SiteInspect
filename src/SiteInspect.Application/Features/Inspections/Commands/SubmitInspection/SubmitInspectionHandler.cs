using MediatR;
using SiteInspect.Application.Common.Abstractions.Identity;
using SiteInspect.Application.Common.Abstractions.Persistence;
using SiteInspect.Application.Common.Authorization;
using SiteInspect.Application.Common.Errors;
using SiteInspect.Application.Common.Persistence;
using SiteInspect.Application.Common.Results;
using SiteInspect.Application.Features.Inspections.Common;
using SiteInspect.Domain.Inspections;
using SiteInspect.Domain.Inspections.InspectionAggregate;

namespace SiteInspect.Application.Features.Inspections.Commands.SubmitInspection;

public sealed class SubmitInspectionHandler(
    ICurrentUser currentUser,
    IInspectionRepository inspectionRepository,
    IUnitOfWork unitOfWork,
    TimeProvider timeProvider)
    : IRequestHandler<SubmitInspectionCommand, Result<InspectionMutationResponse>>
{
    public async Task<Result<InspectionMutationResponse>> Handle(
        SubmitInspectionCommand command,
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
            return Result<InspectionMutationResponse>.Failure(
                InspectionErrors.NotFound(command.InspectionId));
        }

        if (inspection.AssignedInspectorId != inspectorId)
        {
            return Result<InspectionMutationResponse>.Failure(
                InspectionErrors.AssignedInspectorRequired);
        }

        if (inspection.Status != InspectionStatus.InProgress)
        {
            return Result<InspectionMutationResponse>.Failure(InspectionErrors.SubmissionNotAllowed);
        }

        if (inspection.Observations.Any(item => item.IsRequiredSnapshot && item.Outcome is null))
        {
            return Result<InspectionMutationResponse>.Failure(
                InspectionErrors.RequiredObservationsIncomplete);
        }

        if (inspection.Observations.Any(item =>
            item.Outcome == ObservationOutcome.Fail && string.IsNullOrWhiteSpace(item.Notes)))
        {
            return Result<InspectionMutationResponse>.Failure(
                InspectionErrors.FailedObservationNotesRequired);
        }

        if (inspection.Observations.Any(item =>
            item.Outcome == ObservationOutcome.Fail &&
            item.Severity is Severity.High or Severity.Critical &&
            item.Attachments.Count == 0))
        {
            return Result<InspectionMutationResponse>.Failure(InspectionErrors.FailurePhotoRequired);
        }

        inspection.Submit(inspectorId, timeProvider.GetUtcNow());
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<InspectionMutationResponse>.Success(new InspectionMutationResponse(
            inspection.Id,
            inspection.Status,
            inspection.AssignedInspectorId,
            Convert.ToBase64String(inspection.RowVersion)));
    }
}

using MediatR;
using SiteInspect.Application.Common.Abstractions.Identity;
using SiteInspect.Application.Common.Abstractions.Persistence;
using SiteInspect.Application.Common.Authorization;
using SiteInspect.Application.Common.Errors;
using SiteInspect.Application.Common.Persistence;
using SiteInspect.Application.Common.Results;
using SiteInspect.Application.Features.Inspections.Common;
using SiteInspect.Domain.Inspections.InspectionAggregate;

namespace SiteInspect.Application.Features.Inspections.Commands.StartInspection;

public sealed class StartInspectionHandler(
    ICurrentUser currentUser,
    IInspectionRepository inspectionRepository,
    IUnitOfWork unitOfWork,
    TimeProvider timeProvider)
    : IRequestHandler<StartInspectionCommand, Result<InspectionMutationResponse>>
{
    public async Task<Result<InspectionMutationResponse>> Handle(
        StartInspectionCommand command,
        CancellationToken cancellationToken)
    {
        if (!currentUser.IsInRole(RoleNames.Inspector) || currentUser.UserId is not Guid inspectorId)
        {
            return Result<InspectionMutationResponse>.Failure(AuthorizationErrors.Forbidden);
        }

        var inspection = await inspectionRepository.GetForUpdateAsync(
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

        if (inspection.Status is not (InspectionStatus.Assigned or InspectionStatus.InProgress))
        {
            return Result<InspectionMutationResponse>.Failure(InspectionErrors.StartNotAllowed);
        }

        inspection.Start(inspectorId, timeProvider.GetUtcNow());
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<InspectionMutationResponse>.Success(new InspectionMutationResponse(
            inspection.Id,
            inspection.Status,
            inspection.AssignedInspectorId,
            Convert.ToBase64String(inspection.RowVersion)));
    }
}

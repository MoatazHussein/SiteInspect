using MediatR;
using SiteInspect.Application.Common.Abstractions.Identity;
using SiteInspect.Application.Common.Abstractions.Persistence;
using SiteInspect.Application.Common.Authorization;
using SiteInspect.Application.Common.Errors;
using SiteInspect.Application.Common.Persistence;
using SiteInspect.Application.Common.Results;
using SiteInspect.Application.Features.Inspections.Common;
using SiteInspect.Domain.Inspections.InspectionAggregate;

namespace SiteInspect.Application.Features.Inspections.Commands.ReassignInspection;

public sealed class ReassignInspectionHandler(
    ICurrentUser currentUser,
    IInspectionRepository inspectionRepository,
    IInspectorDirectory inspectorDirectory,
    IUnitOfWork unitOfWork)
    : IRequestHandler<ReassignInspectionCommand, Result<InspectionMutationResponse>>
{
    public async Task<Result<InspectionMutationResponse>> Handle(
        ReassignInspectionCommand command,
        CancellationToken cancellationToken)
    {
        if (!currentUser.IsInRole(RoleNames.Manager))
        {
            return Result<InspectionMutationResponse>.Failure(AuthorizationErrors.Forbidden);
        }

        var inspection = await inspectionRepository.GetForUpdateAsync(
            command.InspectionId,
            cancellationToken);

        if (inspection is null)
        {
            return Result<InspectionMutationResponse>.Failure(InspectionErrors.NotFound(command.InspectionId));
        }

        inspection.EnsureCurrentVersion(command.RowVersion);

        if (inspection.Status != InspectionStatus.Assigned)
        {
            return Result<InspectionMutationResponse>.Failure(InspectionErrors.ReassignmentNotAllowed);
        }

        if (!await inspectorDirectory.ExistsAsync(command.InspectorId, cancellationToken))
        {
            return Result<InspectionMutationResponse>.Failure(InspectionErrors.InspectorNotFound);
        }

        inspection.Reassign(command.InspectorId);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<InspectionMutationResponse>.Success(new InspectionMutationResponse(
            inspection.Id,
            inspection.Status,
            inspection.AssignedInspectorId,
            Convert.ToBase64String(inspection.RowVersion)));
    }
}

using MediatR;
using SiteInspect.Application.Common.Abstractions.Identity;
using SiteInspect.Application.Common.Abstractions.Numbering;
using SiteInspect.Application.Common.Abstractions.Persistence;
using SiteInspect.Application.Common.Authorization;
using SiteInspect.Application.Common.Errors;
using SiteInspect.Application.Common.Numbering;
using SiteInspect.Application.Common.Results;
using SiteInspect.Application.Features.Inspections.Common;
using SiteInspect.Domain.Inspections.InspectionAggregate;
using SiteInspect.Domain.Projects.ProjectAggregate;

namespace SiteInspect.Application.Features.Inspections.Commands.CreateInspection;

public sealed class CreateInspectionHandler(
    ICurrentUser currentUser,
    IRepository<Project> projectRepository,
    IRepository<ProjectLocation> locationRepository,
    IInspectionTemplateRepository templateRepository,
    IInspectorDirectory inspectorDirectory,
    INumberSeriesService numberSeriesService,
    IInspectionRepository inspectionRepository,
    IUnitOfWork unitOfWork)
    : IRequestHandler<CreateInspectionCommand, Result<CreateInspectionResponse>>
{
    public async Task<Result<CreateInspectionResponse>> Handle(
        CreateInspectionCommand command,
        CancellationToken cancellationToken)
    {
        if (!currentUser.IsInRole(RoleNames.Manager))
        {
            return Result<CreateInspectionResponse>.Failure(AuthorizationErrors.Forbidden);
        }

        var project = await projectRepository.GetByIdAsync(command.ProjectId, cancellationToken);
        if (project is null || !project.IsActive)
        {
            return Result<CreateInspectionResponse>.Failure(InspectionErrors.ProjectNotFound);
        }

        var location = await locationRepository.GetByIdAsync(command.LocationId, cancellationToken);
        if (location is null || location.ProjectId != project.Id)
        {
            return Result<CreateInspectionResponse>.Failure(InspectionErrors.LocationNotFound);
        }

        var template = await templateRepository.GetActiveWithItemsAsync(command.TemplateId, cancellationToken);
        if (template is null)
        {
            return Result<CreateInspectionResponse>.Failure(InspectionErrors.TemplateNotFound);
        }

        if (!await inspectorDirectory.ExistsAsync(command.InspectorId, cancellationToken))
        {
            return Result<CreateInspectionResponse>.Failure(InspectionErrors.InspectorNotFound);
        }

        var number = await numberSeriesService.NextAsync(
            NumberSeriesDefinitions.Inspections,
            cancellationToken);
        var inspection = Inspection.Create(
            number,
            project.Id,
            location.Id,
            template,
            command.InspectorId,
            command.DueAtUtc);

        await inspectionRepository.AddAsync(inspection, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<CreateInspectionResponse>.Success(new CreateInspectionResponse(
            inspection.Id,
            inspection.Number,
            inspection.Status,
            Convert.ToBase64String(inspection.RowVersion)));
    }
}

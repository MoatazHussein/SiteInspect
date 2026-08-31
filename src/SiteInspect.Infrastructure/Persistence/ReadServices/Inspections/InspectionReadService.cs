using Microsoft.EntityFrameworkCore;
using SiteInspect.Application.Common.Abstractions.Identity;
using SiteInspect.Application.Common.Authorization;
using SiteInspect.Application.Common.Errors;
using SiteInspect.Application.Common.Results;
using SiteInspect.Application.Features.Inspections;
using SiteInspect.Application.Features.Inspections.Common;
using SiteInspect.Application.Features.Inspections.Queries.GetInspection;
using SiteInspect.Application.Features.Inspections.Queries.GetInspectionFilterOptions;
using SiteInspect.Application.Features.Inspections.Queries.GetInspections;
using SiteInspect.Domain.Inspections.InspectionAggregate;
using SiteInspect.Infrastructure.Persistence;

namespace SiteInspect.Infrastructure.Persistence.ReadServices.Inspections;

internal sealed class InspectionReadService(
    ApplicationDbContext dbContext,
    ICurrentUser currentUser) : IInspectionReadService
{
    public async Task<Result<GetInspectionsResponse>> ListAsync(
        GetInspectionsQuery query,
        CancellationToken cancellationToken = default)
    {
        var scopedInspections = GetScopedInspections();
        if (scopedInspections is null)
        {
            return Result<GetInspectionsResponse>.Failure(AuthorizationErrors.Forbidden);
        }

        var filtered = ApplyFilters(scopedInspections, query);
        var totalCount = await filtered.CountAsync(cancellationToken);

        var rows = await (
            from inspection in filtered
            join project in dbContext.Projects.AsNoTracking() on inspection.ProjectId equals project.Id
            join location in dbContext.ProjectLocations.AsNoTracking() on inspection.ProjectLocationId equals location.Id
            join template in dbContext.InspectionTemplates.AsNoTracking() on inspection.TemplateId equals template.Id
            join inspector in dbContext.Users.AsNoTracking() on inspection.AssignedInspectorId equals inspector.Id
            orderby inspection.DueAtUtc, inspection.Number
            select new InspectionListRow(
                inspection.Id,
                inspection.Number,
                project.Name,
                location.Name,
                template.Name,
                inspection.TemplateVersion,
                inspection.AssignedInspectorId,
                inspector.DisplayName,
                inspection.Status,
                inspection.DueAtUtc,
                inspection.RowVersion))
            .Skip((query.Page - 1) * query.PageSize)
            .Take(query.PageSize)
            .ToListAsync(cancellationToken);

        var items = rows.Select(row => new InspectionListItem(
            row.Id,
            row.Number,
            row.ProjectName,
            row.LocationName,
            row.TemplateName,
            row.TemplateVersion,
            row.AssignedInspectorId,
            row.AssignedInspectorName,
            row.Status,
            row.DueAtUtc,
            Convert.ToBase64String(row.RowVersion))).ToArray();

        var totalPages = totalCount == 0
            ? 0
            : (int)Math.Ceiling(totalCount / (double)query.PageSize);

        return Result<GetInspectionsResponse>.Success(
            new GetInspectionsResponse(items, query.Page, query.PageSize, totalCount, totalPages));
    }

    public async Task<Result<GetInspectionResponse>> GetAsync(
        Guid inspectionId,
        CancellationToken cancellationToken = default)
    {
        var scopedInspections = GetScopedInspections();
        if (scopedInspections is null)
        {
            return Result<GetInspectionResponse>.Failure(AuthorizationErrors.Forbidden);
        }

        var row = await (
            from inspection in scopedInspections
            where inspection.Id == inspectionId
            join project in dbContext.Projects.AsNoTracking() on inspection.ProjectId equals project.Id
            join location in dbContext.ProjectLocations.AsNoTracking() on inspection.ProjectLocationId equals location.Id
            join template in dbContext.InspectionTemplates.AsNoTracking() on inspection.TemplateId equals template.Id
            join inspector in dbContext.Users.AsNoTracking() on inspection.AssignedInspectorId equals inspector.Id
            select new InspectionDetailRow(
                inspection.Id,
                inspection.Number,
                inspection.ProjectId,
                project.Name,
                inspection.ProjectLocationId,
                location.Name,
                inspection.TemplateId,
                template.Name,
                inspection.TemplateVersion,
                inspection.AssignedInspectorId,
                inspector.DisplayName,
                inspection.Status,
                inspection.DueAtUtc,
                inspection.StartedAtUtc,
                inspection.SubmittedAtUtc,
                inspection.CompletedAtUtc,
                inspection.CancelledAtUtc,
                inspection.CancellationReason,
                inspection.LastDraftSavedAtUtc,
                inspection.RowVersion))
            .SingleOrDefaultAsync(cancellationToken);

        if (row is null)
        {
            return Result<GetInspectionResponse>.Failure(InspectionErrors.NotFound(inspectionId));
        }

        var observations = await dbContext.InspectionObservations
            .AsNoTracking()
            .Where(observation => observation.InspectionId == inspectionId)
            .OrderBy(observation => observation.DisplayOrderSnapshot)
            .Select(observation => new InspectionObservationResponse(
                observation.Id,
                observation.SectionNameSnapshot,
                observation.QuestionSnapshot,
                observation.DisplayOrderSnapshot,
                observation.IsRequiredSnapshot,
                observation.Outcome,
                observation.Severity,
                observation.Notes,
                observation.ObservedAtUtc))
            .ToArrayAsync(cancellationToken);

        return Result<GetInspectionResponse>.Success(new GetInspectionResponse(
            row.Id,
            row.Number,
            row.ProjectId,
            row.ProjectName,
            row.LocationId,
            row.LocationName,
            row.TemplateId,
            row.TemplateName,
            row.TemplateVersion,
            row.AssignedInspectorId,
            row.AssignedInspectorName,
            row.Status,
            row.DueAtUtc,
            row.StartedAtUtc,
            row.SubmittedAtUtc,
            row.CompletedAtUtc,
            row.CancelledAtUtc,
            row.CancellationReason,
            row.LastDraftSavedAtUtc,
            Convert.ToBase64String(row.RowVersion),
            observations));
    }

    public async Task<Result<GetInspectionFilterOptionsResponse>> GetFilterOptionsAsync(
        CancellationToken cancellationToken = default)
    {
        var scopedInspections = GetScopedInspections();
        if (scopedInspections is null)
        {
            return Result<GetInspectionFilterOptionsResponse>.Failure(AuthorizationErrors.Forbidden);
        }

        var projects = await dbContext.Projects
            .AsNoTracking()
            .Where(project => scopedInspections.Any(inspection => inspection.ProjectId == project.Id))
            .OrderBy(project => project.Name)
            .Select(project => new FilterOption<Guid>(project.Id, project.Name))
            .ToArrayAsync(cancellationToken);

        var locations = await dbContext.ProjectLocations
            .AsNoTracking()
            .Where(location => scopedInspections.Any(inspection => inspection.ProjectLocationId == location.Id))
            .OrderBy(location => location.Name)
            .Select(location => new FilterOption<Guid>(location.Id, location.Name))
            .ToArrayAsync(cancellationToken);

        var inspectors = await dbContext.Users
            .AsNoTracking()
            .Where(inspector => scopedInspections.Any(inspection => inspection.AssignedInspectorId == inspector.Id))
            .OrderBy(inspector => inspector.DisplayName)
            .Select(inspector => new FilterOption<Guid>(inspector.Id, inspector.DisplayName))
            .ToArrayAsync(cancellationToken);

        var statuses = Enum.GetValues<InspectionStatus>()
            .Select(status => new FilterOption<string>(status.ToString(), status.ToString()))
            .ToArray();

        return Result<GetInspectionFilterOptionsResponse>.Success(
            new GetInspectionFilterOptionsResponse(projects, locations, inspectors, statuses));
    }

    private IQueryable<Inspection>? GetScopedInspections()
    {
        var inspections = dbContext.Inspections.AsNoTracking();

        if (currentUser.IsInRole(RoleNames.Manager))
        {
            return inspections;
        }

        if (currentUser.IsInRole(RoleNames.Inspector) && currentUser.UserId is Guid userId)
        {
            return inspections.Where(inspection => inspection.AssignedInspectorId == userId);
        }

        return null;
    }

    private IQueryable<Inspection> ApplyFilters(
        IQueryable<Inspection> inspections,
        GetInspectionsQuery query)
    {
        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var search = query.Search.Trim();
            inspections = inspections.Where(inspection =>
                inspection.Number.Contains(search) ||
                dbContext.Projects.Any(project =>
                    project.Id == inspection.ProjectId && project.Name.Contains(search)) ||
                dbContext.ProjectLocations.Any(location =>
                    location.Id == inspection.ProjectLocationId && location.Name.Contains(search)));
        }

        if (query.ProjectId is Guid projectId)
        {
            inspections = inspections.Where(inspection => inspection.ProjectId == projectId);
        }

        if (query.LocationId is Guid locationId)
        {
            inspections = inspections.Where(inspection => inspection.ProjectLocationId == locationId);
        }

        if (query.InspectorId is Guid inspectorId)
        {
            inspections = inspections.Where(inspection => inspection.AssignedInspectorId == inspectorId);
        }

        if (query.Status is InspectionStatus status)
        {
            inspections = inspections.Where(inspection => inspection.Status == status);
        }

        if (query.DueFromUtc is DateTimeOffset dueFromUtc)
        {
            inspections = inspections.Where(inspection => inspection.DueAtUtc >= dueFromUtc);
        }

        if (query.DueToUtc is DateTimeOffset dueToUtc)
        {
            inspections = inspections.Where(inspection => inspection.DueAtUtc <= dueToUtc);
        }

        return inspections;
    }

    private sealed record InspectionListRow(
        Guid Id,
        string Number,
        string ProjectName,
        string LocationName,
        string TemplateName,
        int TemplateVersion,
        Guid AssignedInspectorId,
        string AssignedInspectorName,
        InspectionStatus Status,
        DateTimeOffset DueAtUtc,
        byte[] RowVersion);

    private sealed record InspectionDetailRow(
        Guid Id,
        string Number,
        Guid ProjectId,
        string ProjectName,
        Guid LocationId,
        string LocationName,
        Guid TemplateId,
        string TemplateName,
        int TemplateVersion,
        Guid AssignedInspectorId,
        string AssignedInspectorName,
        InspectionStatus Status,
        DateTimeOffset DueAtUtc,
        DateTimeOffset? StartedAtUtc,
        DateTimeOffset? SubmittedAtUtc,
        DateTimeOffset? CompletedAtUtc,
        DateTimeOffset? CancelledAtUtc,
        string? CancellationReason,
        DateTimeOffset? LastDraftSavedAtUtc,
        byte[] RowVersion);
}

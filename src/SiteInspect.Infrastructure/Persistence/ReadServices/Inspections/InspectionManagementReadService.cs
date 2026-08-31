using Microsoft.EntityFrameworkCore;
using SiteInspect.Application.Common.Authorization;
using SiteInspect.Application.Common.Results;
using SiteInspect.Application.Features.Inspections.Common;
using SiteInspect.Application.Features.Inspections.Queries.GetInspectionManagementOptions;

namespace SiteInspect.Infrastructure.Persistence.ReadServices.Inspections;

internal sealed class InspectionManagementReadService(ApplicationDbContext dbContext)
    : IInspectionManagementReadService
{
    public async Task<Result<GetInspectionManagementOptionsResponse>> GetOptionsAsync(
        CancellationToken cancellationToken = default)
    {
        var projects = await dbContext.Projects
            .AsNoTracking()
            .Where(project => project.IsActive)
            .OrderBy(project => project.Name)
            .Select(project => new ManagementProjectOption(project.Id, project.Name))
            .ToArrayAsync(cancellationToken);

        var projectIds = projects.Select(project => project.Id).ToArray();
        var locations = await dbContext.ProjectLocations
            .AsNoTracking()
            .Where(location => projectIds.Contains(location.ProjectId))
            .OrderBy(location => location.Name)
            .Select(location => new ManagementLocationOption(location.Id, location.ProjectId, location.Name))
            .ToArrayAsync(cancellationToken);

        var templates = await dbContext.InspectionTemplates
            .AsNoTracking()
            .Where(template => template.IsActive && template.Items.Any())
            .OrderBy(template => template.Name)
            .ThenByDescending(template => template.Version)
            .Select(template => new ManagementTemplateOption(template.Id, template.Name, template.Version))
            .ToArrayAsync(cancellationToken);

        var inspectors = await (
            from user in dbContext.Users.AsNoTracking()
            join userRole in dbContext.UserRoles.AsNoTracking() on user.Id equals userRole.UserId
            join role in dbContext.Roles.AsNoTracking() on userRole.RoleId equals role.Id
            where role.Name == RoleNames.Inspector
            orderby user.DisplayName
            select new ManagementInspectorOption(user.Id, user.DisplayName))
            .ToArrayAsync(cancellationToken);

        return Result<GetInspectionManagementOptionsResponse>.Success(
            new GetInspectionManagementOptionsResponse(projects, locations, templates, inspectors));
    }
}

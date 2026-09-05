using Microsoft.EntityFrameworkCore;
using SiteInspect.Domain.Projects.ProjectAggregate;

namespace SiteInspect.Infrastructure.Persistence.Initialization.Steps;

internal sealed class ProjectsSeedStep(ApplicationDbContext dbContext) : ISeedStep
{
    public string Name => "Projects and locations";
    public int Order => SeedStepOrder.Projects;

    public async Task SeedAsync(CancellationToken cancellationToken)
    {
        var project = new Project(DemoSeedData.ProjectId, "Harbor View Apartments", "APT-001", true);
        var locations = new ProjectLocation[]
        {
            new ProjectLocation(DemoSeedData.BuildingId, DemoSeedData.ProjectId, "Building A", null),
            new ProjectLocation(DemoSeedData.FloorId, DemoSeedData.ProjectId, "Floor 5", DemoSeedData.BuildingId),
            new ProjectLocation(DemoSeedData.CorridorId, DemoSeedData.ProjectId, "Eastern Corridor", DemoSeedData.FloorId),
        };

        if (!await dbContext.Projects.AnyAsync(item => item.Id == DemoSeedData.ProjectId, cancellationToken))
        {
            dbContext.Projects.Add(project);
        }

        var locationIds = locations.Select(location => location.Id).ToArray();
        var existingLocationIds = await dbContext.ProjectLocations
            .Where(location => locationIds.Contains(location.Id))
            .Select(location => location.Id)
            .ToArrayAsync(cancellationToken);
        dbContext.ProjectLocations.AddRange(
            locations.Where(location => !existingLocationIds.Contains(location.Id)));

        await dbContext.SaveChangesAsync(cancellationToken);
    }
}

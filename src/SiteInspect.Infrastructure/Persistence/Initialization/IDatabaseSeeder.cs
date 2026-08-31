namespace SiteInspect.Infrastructure.Persistence.Initialization;

public interface IDatabaseSeeder
{
    Task SeedAsync(CancellationToken cancellationToken = default);
}

namespace SiteInspect.Infrastructure.Persistence.Initialization;

public interface IDatabaseMigrator
{
    Task MigrateAsync(CancellationToken cancellationToken = default);
}

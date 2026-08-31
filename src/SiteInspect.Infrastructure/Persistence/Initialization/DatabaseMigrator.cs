using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SiteInspect.Infrastructure.Persistence;

namespace SiteInspect.Infrastructure.Persistence.Initialization;

internal sealed partial class DatabaseMigrator(
    ApplicationDbContext dbContext,
    ILogger<DatabaseMigrator> logger) : IDatabaseMigrator
{
    public async Task MigrateAsync(CancellationToken cancellationToken)
    {
        LogApplyingMigrations(logger);
        await dbContext.Database.MigrateAsync(cancellationToken);
    }

    [LoggerMessage(Level = LogLevel.Information, Message = "Applying SiteInspect database migrations")]
    private static partial void LogApplyingMigrations(ILogger logger);
}

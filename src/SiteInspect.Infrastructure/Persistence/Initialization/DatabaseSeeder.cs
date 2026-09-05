using Microsoft.Extensions.Logging;

namespace SiteInspect.Infrastructure.Persistence.Initialization;

internal sealed partial class DatabaseSeeder(
    IEnumerable<ISeedStep> steps,
    ILogger<DatabaseSeeder> logger) : IDatabaseSeeder
{
    public async Task SeedAsync(CancellationToken cancellationToken)
    {
        // Steps share a scoped DbContext and depend on earlier steps being persisted.
        foreach (var step in steps.OrderBy(step => step.Order))
        {
            cancellationToken.ThrowIfCancellationRequested();
            LogSeedStepStarting(logger, step.Name, step.Order);
            await step.SeedAsync(cancellationToken);
        }

        LogDemoDataReady(logger);
    }

    [LoggerMessage(Level = LogLevel.Information, Message = "Running seed step {StepName} (order {StepOrder})")]
    private static partial void LogSeedStepStarting(ILogger logger, string stepName, int stepOrder);

    [LoggerMessage(Level = LogLevel.Information, Message = "SiteInspect demo data is ready")]
    private static partial void LogDemoDataReady(ILogger logger);
}

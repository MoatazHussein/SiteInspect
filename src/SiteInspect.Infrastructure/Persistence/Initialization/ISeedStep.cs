namespace SiteInspect.Infrastructure.Persistence.Initialization;

internal interface ISeedStep
{
    string Name { get; }
    int Order { get; }
    Task SeedAsync(CancellationToken cancellationToken);
}

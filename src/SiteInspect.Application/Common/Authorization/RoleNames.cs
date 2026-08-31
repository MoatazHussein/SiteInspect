namespace SiteInspect.Application.Common.Authorization;

public static class RoleNames
{
    public const string Manager = "Manager";
    public const string Inspector = "Inspector";
    public const string Contractor = "Contractor";

    public static IReadOnlyCollection<string> All { get; } =
        [Manager, Inspector, Contractor];
}

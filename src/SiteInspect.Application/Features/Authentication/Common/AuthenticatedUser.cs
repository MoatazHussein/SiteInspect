namespace SiteInspect.Application.Features.Authentication.Common;

public sealed record AuthenticatedUser(
    Guid Id,
    string Email,
    string DisplayName,
    IReadOnlyCollection<string> Roles);

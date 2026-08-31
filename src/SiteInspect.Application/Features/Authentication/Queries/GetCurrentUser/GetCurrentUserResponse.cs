namespace SiteInspect.Application.Features.Authentication.Queries.GetCurrentUser;

public sealed record GetCurrentUserResponse(
    Guid Id,
    string Email,
    string DisplayName,
    IReadOnlyCollection<string> Roles);

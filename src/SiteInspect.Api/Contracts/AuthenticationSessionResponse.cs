using SiteInspect.Application.Features.Authentication.Common;

namespace SiteInspect.Api.Contracts;

public sealed record AuthenticationSessionResponse(
    string AccessToken,
    DateTimeOffset AccessTokenExpiresAtUtc,
    AuthenticatedUser User);

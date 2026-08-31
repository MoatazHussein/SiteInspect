namespace SiteInspect.Infrastructure.Identity.Options;

public sealed class JwtOptions
{
    public string Issuer { get; init; } = "SiteInspect";

    public string Audience { get; init; } = "SiteInspect.Web";

    public string SigningKey { get; init; } = string.Empty;

    public int AccessTokenMinutes { get; init; } = 30;

    public int RefreshTokenDays { get; init; } = 7;
}

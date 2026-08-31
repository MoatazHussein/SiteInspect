namespace SiteInspect.Api.Contracts;

public sealed record ApiErrorResponse(
    string Code,
    string Message);

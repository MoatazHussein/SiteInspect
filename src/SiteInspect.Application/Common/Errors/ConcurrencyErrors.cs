using SiteInspect.Application.Common.Results;

namespace SiteInspect.Application.Common.Errors;

public static class ConcurrencyErrors
{
    public static readonly Error VersionConflict = new(
        "Concurrency.VersionConflict",
        "The resource changed since it was loaded. Refresh and try again.",
        ErrorType.Conflict);
}

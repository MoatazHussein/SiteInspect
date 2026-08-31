using SiteInspect.Application.Common.Results;

namespace SiteInspect.Application.Common.Errors;

public static class CommonErrors
{
    public static readonly Error Validation = new(
        "Common.ValidationError",
        "One or more validation errors occurred.",
        ErrorType.Validation);

    public static readonly Error Unexpected = new(
        "Common.UnexpectedError",
        "An unexpected error occurred.",
        ErrorType.Failure);
}

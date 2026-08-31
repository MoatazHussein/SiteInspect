using SiteInspect.Api.Contracts;
using SiteInspect.Api.Diagnostics;
using SiteInspect.Application.Common.Results;

namespace SiteInspect.Api.ErrorHandling;

internal static class ApiResponseFactory
{
    public static ApiResponse<T> Success<T>(HttpContext httpContext, T? data) => new(
        true,
        data,
        Array.Empty<ApiErrorResponse>(),
        GetCorrelationId(httpContext));

    public static ApiResponse<T> Failure<T>(
        HttpContext httpContext,
        IReadOnlyCollection<Error> errors)
    {
        ArgumentNullException.ThrowIfNull(errors);

        if (errors.Count == 0)
        {
            throw new ArgumentException("At least one error is required.", nameof(errors));
        }

        return new ApiResponse<T>(
            false,
            default,
            errors.Select(error => new ApiErrorResponse(error.Code, error.Message)).ToArray(),
            GetCorrelationId(httpContext));
    }

    public static int MapStatus(ErrorType errorType) => errorType switch
    {
        ErrorType.Validation => StatusCodes.Status400BadRequest,
        ErrorType.Unauthorized => StatusCodes.Status401Unauthorized,
        ErrorType.Forbidden => StatusCodes.Status403Forbidden,
        ErrorType.NotFound => StatusCodes.Status404NotFound,
        ErrorType.Conflict => StatusCodes.Status409Conflict,
        ErrorType.Failure => StatusCodes.Status422UnprocessableEntity,
        _ => throw new ArgumentOutOfRangeException(nameof(errorType), errorType, null),
    };

    private static string? GetCorrelationId(HttpContext httpContext) =>
        httpContext.Items.TryGetValue(CorrelationIdMiddleware.HttpContextItemKey, out var correlationId)
            ? correlationId as string
            : null;
}

using SiteInspect.Application.Common.Errors;
using SiteInspect.Application.Common.Results;

namespace SiteInspect.Api.ErrorHandling;

internal static class ApiErrors
{
    public static readonly Error BadRequest = new(
        "Api.BadRequest",
        "The request is invalid.",
        ErrorType.Validation);

    public static readonly Error EndpointNotFound = new(
        "Api.EndpointNotFound",
        "The requested API endpoint was not found.",
        ErrorType.NotFound);

    public static readonly Error MethodNotAllowed = new(
        "Api.MethodNotAllowed",
        "The HTTP method is not allowed for this endpoint.",
        ErrorType.Validation);

    public static readonly Error UnsupportedMediaType = new(
        "Api.UnsupportedMediaType",
        "The request content type is not supported.",
        ErrorType.Validation);

    public static readonly Error RequestFailed = new(
        "Api.RequestFailed",
        "The request could not be completed.",
        ErrorType.Failure);

    public static Error FromStatusCode(int statusCode) => statusCode switch
    {
        StatusCodes.Status400BadRequest => BadRequest,
        StatusCodes.Status401Unauthorized => AuthenticationErrors.AuthenticationRequired,
        StatusCodes.Status403Forbidden => AuthorizationErrors.Forbidden,
        StatusCodes.Status404NotFound => EndpointNotFound,
        StatusCodes.Status405MethodNotAllowed => MethodNotAllowed,
        StatusCodes.Status415UnsupportedMediaType => UnsupportedMediaType,
        _ => RequestFailed,
    };
}

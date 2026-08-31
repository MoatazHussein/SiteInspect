using Microsoft.AspNetCore.Mvc;
using SiteInspect.Application.Common.Results;

namespace SiteInspect.Api.ErrorHandling;

internal static class ResultActionExtensions
{
    public static ObjectResult ToActionResult(
        this Result result,
        HttpContext httpContext,
        int successStatusCode = StatusCodes.Status200OK) =>
        result.IsSuccess
            ? new ObjectResult(ApiResponseFactory.Success<object?>(httpContext, null))
            {
                StatusCode = successStatusCode,
            }
            : result.ToErrorActionResult(httpContext);

    public static ObjectResult ToActionResult<T>(
        this Result<T> result,
        HttpContext httpContext,
        int successStatusCode = StatusCodes.Status200OK) =>
        result.IsSuccess
            ? new ObjectResult(ApiResponseFactory.Success(httpContext, result.Value))
            {
                StatusCode = successStatusCode,
            }
            : result.ToErrorActionResult(httpContext);

    private static ObjectResult ToErrorActionResult(this Result result, HttpContext httpContext)
    {
        if (result.IsSuccess)
        {
            throw new InvalidOperationException("A successful result cannot be mapped to an error response.");
        }

        var statusCode = ApiResponseFactory.MapStatus(result.Errors.First().Type);
        var response = ApiResponseFactory.Failure<object?>(httpContext, result.Errors);

        return new ObjectResult(response)
        {
            StatusCode = statusCode,
        };
    }
}

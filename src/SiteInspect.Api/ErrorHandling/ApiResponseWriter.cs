using SiteInspect.Api.Contracts;
using SiteInspect.Application.Common.Results;

namespace SiteInspect.Api.ErrorHandling;

internal static class ApiResponseWriter
{
    public static Task WriteFailureAsync(
        HttpContext httpContext,
        int statusCode,
        IReadOnlyCollection<Error> errors,
        CancellationToken cancellationToken = default)
    {
        if (httpContext.Response.HasStarted)
        {
            return Task.CompletedTask;
        }

        var response = ApiResponseFactory.Failure<object?>(httpContext, errors);
        httpContext.Response.StatusCode = statusCode;

        return httpContext.Response.WriteAsJsonAsync<ApiResponse<object?>>(
            response,
            cancellationToken);
    }
}

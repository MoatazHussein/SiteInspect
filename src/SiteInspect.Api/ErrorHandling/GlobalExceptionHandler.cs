using Microsoft.AspNetCore.Diagnostics;
using SiteInspect.Application.Common.Errors;
using SiteInspect.Application.Common.Exceptions;

namespace SiteInspect.Api.ErrorHandling;

internal sealed partial class GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        if (exception is PersistenceConcurrencyException)
        {
            LogConcurrencyConflict(logger, httpContext.Request.Method, httpContext.Request.Path);

            await ApiResponseWriter.WriteFailureAsync(
                httpContext,
                StatusCodes.Status409Conflict,
                [ConcurrencyErrors.VersionConflict],
                cancellationToken);

            return true;
        }

        if (exception is RequestValidationException validationException)
        {
            var statusCode = ApiResponseFactory.MapStatus(validationException.Errors.First().Type);
            LogValidationFailure(
                logger,
                httpContext.Request.Method,
                httpContext.Request.Path,
                statusCode);

            await ApiResponseWriter.WriteFailureAsync(
                httpContext,
                statusCode,
                validationException.Errors,
                cancellationToken);

            return true;
        }

        if (exception is BadHttpRequestException badRequestException)
        {
            LogBadRequest(
                logger,
                httpContext.Request.Method,
                httpContext.Request.Path,
                badRequestException.Message);

            await ApiResponseWriter.WriteFailureAsync(
                httpContext,
                badRequestException.StatusCode,
                [ApiErrors.BadRequest],
                cancellationToken);

            return true;
        }

        LogUnhandledException(logger, httpContext.Request.Method, httpContext.Request.Path, exception);

        await ApiResponseWriter.WriteFailureAsync(
            httpContext,
            StatusCodes.Status500InternalServerError,
            [CommonErrors.Unexpected],
            cancellationToken);

        return true;
    }

    [LoggerMessage(
        EventId = 4000,
        Level = LogLevel.Warning,
        Message = "Invalid request while processing {Method} {Path}: {Reason}")]
    private static partial void LogBadRequest(
        ILogger logger,
        string method,
        PathString path,
        string reason);

    [LoggerMessage(
        EventId = 4001,
        Level = LogLevel.Warning,
        Message = "Request validation rejected {Method} {Path} with status code {StatusCode}")]
    private static partial void LogValidationFailure(
        ILogger logger,
        string method,
        PathString path,
        int statusCode);

    [LoggerMessage(
        EventId = 4090,
        Level = LogLevel.Warning,
        Message = "Concurrency conflict while processing {Method} {Path}")]
    private static partial void LogConcurrencyConflict(
        ILogger logger,
        string method,
        PathString path);

    [LoggerMessage(
        EventId = 5000,
        Level = LogLevel.Error,
        Message = "Unhandled exception while processing {Method} {Path}")]
    private static partial void LogUnhandledException(
        ILogger logger,
        string method,
        PathString path,
        Exception exception);
}

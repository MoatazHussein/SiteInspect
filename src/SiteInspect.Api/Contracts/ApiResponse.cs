namespace SiteInspect.Api.Contracts;

public sealed record ApiResponse<T>(
    bool IsSuccess,
    T? Data,
    IReadOnlyCollection<ApiErrorResponse> Errors,
    string? CorrelationId);

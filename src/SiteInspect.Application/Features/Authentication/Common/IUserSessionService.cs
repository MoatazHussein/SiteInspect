using SiteInspect.Application.Common.Results;

namespace SiteInspect.Application.Features.Authentication.Common;

public interface IUserSessionService
{
    Task<Result<AuthenticatedSession>> LoginAsync(
        string email,
        string password,
        CancellationToken cancellationToken = default);

    Task<Result<AuthenticatedSession>> RefreshAsync(
        string refreshToken,
        CancellationToken cancellationToken = default);

    Task RevokeAsync(string? refreshToken, CancellationToken cancellationToken = default);

    Task<Result<AuthenticatedUser>> GetCurrentUserAsync(
        Guid userId,
        CancellationToken cancellationToken = default);
}

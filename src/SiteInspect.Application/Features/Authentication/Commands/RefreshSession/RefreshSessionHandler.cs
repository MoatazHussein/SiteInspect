using MediatR;
using SiteInspect.Application.Common.Results;
using SiteInspect.Application.Features.Authentication.Common;

namespace SiteInspect.Application.Features.Authentication.Commands.RefreshSession;

public sealed class RefreshSessionHandler(IUserSessionService sessionService)
    : IRequestHandler<RefreshSessionCommand, Result<RefreshSessionResponse>>
{
    public async Task<Result<RefreshSessionResponse>> Handle(
        RefreshSessionCommand command,
        CancellationToken cancellationToken)
    {
        var result = await sessionService.RefreshAsync(command.RefreshToken, cancellationToken);
        return result.IsFailure
            ? Result<RefreshSessionResponse>.Failure(result.Errors)
            : Result<RefreshSessionResponse>.Success(Map(result.Value));
    }

    private static RefreshSessionResponse Map(AuthenticatedSession session) => new(
        session.AccessToken,
        session.AccessTokenExpiresAtUtc,
        session.RefreshToken,
        session.RefreshTokenExpiresAtUtc,
        session.User);
}

using MediatR;
using SiteInspect.Application.Common.Results;
using SiteInspect.Application.Features.Authentication.Common;

namespace SiteInspect.Application.Features.Authentication.Commands.Login;

public sealed class LoginHandler(IUserSessionService sessionService)
    : IRequestHandler<LoginCommand, Result<LoginResponse>>
{
    public async Task<Result<LoginResponse>> Handle(
        LoginCommand command,
        CancellationToken cancellationToken)
    {
        var result = await sessionService.LoginAsync(command.Email, command.Password, cancellationToken);
        return result.IsFailure
            ? Result<LoginResponse>.Failure(result.Errors)
            : Result<LoginResponse>.Success(Map(result.Value));
    }

    private static LoginResponse Map(AuthenticatedSession session) => new(
        session.AccessToken,
        session.AccessTokenExpiresAtUtc,
        session.RefreshToken,
        session.RefreshTokenExpiresAtUtc,
        session.User);
}

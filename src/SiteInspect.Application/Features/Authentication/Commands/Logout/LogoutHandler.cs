using MediatR;
using SiteInspect.Application.Common.Results;
using SiteInspect.Application.Features.Authentication.Common;

namespace SiteInspect.Application.Features.Authentication.Commands.Logout;

public sealed class LogoutHandler(IUserSessionService sessionService)
    : IRequestHandler<LogoutCommand, Result>
{
    public async Task<Result> Handle(
        LogoutCommand command,
        CancellationToken cancellationToken)
    {
        await sessionService.RevokeAsync(command.RefreshToken, cancellationToken);
        return Result.Success();
    }
}

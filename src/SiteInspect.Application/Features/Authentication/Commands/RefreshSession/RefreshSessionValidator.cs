using FluentValidation;
using SiteInspect.Application.Common.Errors;

namespace SiteInspect.Application.Features.Authentication.Commands.RefreshSession;

public sealed class RefreshSessionValidator : AbstractValidator<RefreshSessionCommand>
{
    public RefreshSessionValidator()
    {
        RuleFor(command => command.RefreshToken)
            .NotEmpty()
            .WithMessage(AuthenticationErrors.InvalidRefreshToken.Message)
            .WithErrorCode(AuthenticationErrors.InvalidRefreshToken.Code)
            .WithState(_ => AuthenticationErrors.InvalidRefreshToken);
    }
}

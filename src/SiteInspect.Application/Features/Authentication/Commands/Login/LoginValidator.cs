using SiteInspect.Application.Common.Errors;
using FluentValidation;

namespace SiteInspect.Application.Features.Authentication.Commands.Login;

public sealed class LoginValidator : AbstractValidator<LoginCommand>
{
    public LoginValidator()
    {
        RuleFor(command => command)
            .Must(command =>
                !string.IsNullOrWhiteSpace(command.Email) &&
                !string.IsNullOrWhiteSpace(command.Password))
            .WithMessage(AuthenticationErrors.InvalidCredentials.Message)
            .WithErrorCode(AuthenticationErrors.InvalidCredentials.Code)
            .WithState(_ => AuthenticationErrors.InvalidCredentials);
    }
}

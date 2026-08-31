using SiteInspect.Application.Common.Errors;
using FluentValidation;

namespace SiteInspect.Application.Features.Authentication.Queries.GetCurrentUser;

public sealed class GetCurrentUserValidator : AbstractValidator<GetCurrentUserQuery>
{
    public GetCurrentUserValidator()
    {
        RuleFor(query => query.UserId)
            .NotEmpty()
            .WithMessage(AuthenticationErrors.AuthenticationRequired.Message)
            .WithErrorCode(AuthenticationErrors.AuthenticationRequired.Code)
            .WithState(_ => AuthenticationErrors.AuthenticationRequired);
    }
}

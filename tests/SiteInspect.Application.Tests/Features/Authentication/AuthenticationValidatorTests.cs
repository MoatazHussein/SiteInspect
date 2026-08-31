using FluentAssertions;
using SiteInspect.Application.Common.Errors;
using SiteInspect.Application.Common.Validation;
using SiteInspect.Application.Features.Authentication.Commands.Login;
using SiteInspect.Application.Features.Authentication.Commands.RefreshSession;
using SiteInspect.Application.Features.Authentication.Queries.GetCurrentUser;

namespace SiteInspect.Application.Tests.Features.Authentication;

public sealed class AuthenticationValidatorTests
{
    [Fact]
    public void LoginValidatorPreservesInvalidCredentialsError()
    {
        var validation = new LoginValidator().Validate(new LoginCommand(string.Empty, string.Empty));

        validation.IsValid.Should().BeFalse();
        validation.ToErrors().Should().ContainSingle().Which
            .Should().Be(AuthenticationErrors.InvalidCredentials);
    }

    [Fact]
    public void RefreshSessionValidatorPreservesInvalidRefreshTokenError()
    {
        var validation = new RefreshSessionValidator().Validate(new RefreshSessionCommand(string.Empty));

        validation.IsValid.Should().BeFalse();
        validation.ToErrors().Should().ContainSingle().Which
            .Should().Be(AuthenticationErrors.InvalidRefreshToken);
    }

    [Fact]
    public void GetCurrentUserValidatorAcceptsAUserId()
    {
        var validation = new GetCurrentUserValidator().Validate(new GetCurrentUserQuery(Guid.NewGuid()));

        validation.IsValid.Should().BeTrue();
    }
}

using FluentAssertions;
using SiteInspect.Application.Common.Results;

namespace SiteInspect.Application.Tests.Common.Results;

public sealed class ResultTests
{
    private static readonly Error ValidationError = new(
        "Test.Invalid",
        "The value is invalid.",
        ErrorType.Validation);

    [Fact]
    public void SuccessHasNoErrors()
    {
        var result = Result.Success();

        result.IsSuccess.Should().BeTrue();
        result.IsFailure.Should().BeFalse();
        result.Errors.Should().BeEmpty();
    }

    [Fact]
    public void FailurePreservesErrors()
    {
        var result = Result.Failure(ValidationError);

        result.IsFailure.Should().BeTrue();
        result.Errors.Should().ContainSingle().Which.Should().Be(ValidationError);
    }

    [Fact]
    public void GenericSuccessExposesValue()
    {
        var result = Result<string>.Success("site-inspect");

        result.Value.Should().Be("site-inspect");
    }

    [Fact]
    public void GenericFailureDoesNotExposeValue()
    {
        var result = Result<string>.Failure(ValidationError);

        var accessValue = () => result.Value;

        accessValue.Should().Throw<InvalidOperationException>();
    }
}

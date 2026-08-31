using FluentAssertions;
using Microsoft.AspNetCore.Http;
using SiteInspect.Api.Diagnostics;
using SiteInspect.Api.ErrorHandling;
using SiteInspect.Application.Common.Results;

namespace SiteInspect.IntegrationTests.Api;

public sealed class ApiResponseFactoryTests
{
    [Fact]
    public void SuccessCreatesUnifiedResponseWithDataAndCorrelationId()
    {
        var context = CreateHttpContext();
        var data = new { Id = Guid.CreateVersion7(), Name = "Inspection" };

        var response = ApiResponseFactory.Success(context, data);

        response.IsSuccess.Should().BeTrue();
        response.Data.Should().BeSameAs(data);
        response.Errors.Should().BeEmpty();
        response.CorrelationId.Should().Be("correlation-456");
    }

    [Fact]
    public void FailurePreservesApplicationError()
    {
        var context = CreateHttpContext();
        var error = new Error(
            "Inspections.NotFound",
            "Inspection was not found.",
            ErrorType.NotFound);

        var response = ApiResponseFactory.Failure<object?>(context, [error]);

        response.IsSuccess.Should().BeFalse();
        response.Data.Should().BeNull();
        var responseError = response.Errors.Should().ContainSingle().Which;
        responseError.Code.Should().Be("Inspections.NotFound");
        responseError.Message.Should().Be("Inspection was not found.");
        response.CorrelationId.Should().Be("correlation-456");
        ApiResponseFactory.MapStatus(error.Type).Should().Be(StatusCodes.Status404NotFound);
    }

    private static DefaultHttpContext CreateHttpContext()
    {
        var context = new DefaultHttpContext();

        context.Items[CorrelationIdMiddleware.HttpContextItemKey] = "correlation-456";
        return context;
    }
}

using FluentAssertions;
using SiteInspect.Application.Common.Validation;
using SiteInspect.Application.Features.Inspections;
using SiteInspect.Application.Features.Inspections.Queries.GetInspection;
using SiteInspect.Application.Features.Inspections.Queries.GetInspections;

namespace SiteInspect.Application.Tests.Features.Inspections;

public sealed class InspectionValidatorTests
{
    [Fact]
    public void GetInspectionsValidatorReturnsOneStableErrorForInvalidPaging()
    {
        var validation = new GetInspectionsValidator().Validate(new GetInspectionsQuery(0, 101));

        validation.IsValid.Should().BeFalse();
        validation.ToErrors().Should().ContainSingle().Which
            .Should().Be(InspectionErrors.InvalidPaging);
    }

    [Fact]
    public void GetInspectionValidatorPreservesNotFoundErrorForEmptyId()
    {
        var validation = new GetInspectionValidator().Validate(new GetInspectionQuery(Guid.Empty));

        validation.IsValid.Should().BeFalse();
        validation.ToErrors().Should().ContainSingle().Which
            .Should().Be(InspectionErrors.NotFound(Guid.Empty));
    }
}

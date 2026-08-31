using FluentValidation;

namespace SiteInspect.Application.Features.Inspections.Queries.GetInspections;

public sealed class GetInspectionsValidator : AbstractValidator<GetInspectionsQuery>
{
    public GetInspectionsValidator()
    {
        RuleFor(query => query.Page)
            .GreaterThanOrEqualTo(1)
            .WithMessage(InspectionErrors.InvalidPaging.Message)
            .WithErrorCode(InspectionErrors.InvalidPaging.Code)
            .WithState(_ => InspectionErrors.InvalidPaging);

        RuleFor(query => query.PageSize)
            .InclusiveBetween(1, 100)
            .WithMessage(InspectionErrors.InvalidPaging.Message)
            .WithErrorCode(InspectionErrors.InvalidPaging.Code)
            .WithState(_ => InspectionErrors.InvalidPaging);
    }
}

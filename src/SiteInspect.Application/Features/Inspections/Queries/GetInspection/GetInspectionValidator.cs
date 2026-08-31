using FluentValidation;

namespace SiteInspect.Application.Features.Inspections.Queries.GetInspection;

public sealed class GetInspectionValidator : AbstractValidator<GetInspectionQuery>
{
    public GetInspectionValidator()
    {
        RuleFor(query => query.InspectionId)
            .NotEmpty()
            .WithMessage(InspectionErrors.NotFound(Guid.Empty).Message)
            .WithErrorCode(InspectionErrors.NotFound(Guid.Empty).Code)
            .WithState(_ => InspectionErrors.NotFound(Guid.Empty));
    }
}

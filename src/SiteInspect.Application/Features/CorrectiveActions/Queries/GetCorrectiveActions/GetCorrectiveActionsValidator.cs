using FluentValidation;

namespace SiteInspect.Application.Features.CorrectiveActions.Queries.GetCorrectiveActions;

public sealed class GetCorrectiveActionsValidator : AbstractValidator<GetCorrectiveActionsQuery>
{
    public GetCorrectiveActionsValidator()
    {
        RuleFor(query => query.InspectionId).NotEmpty();
    }
}

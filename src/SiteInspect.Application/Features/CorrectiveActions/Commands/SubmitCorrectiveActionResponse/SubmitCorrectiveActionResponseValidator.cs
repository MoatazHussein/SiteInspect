using FluentValidation;
using SiteInspect.Application.Common.Persistence;

namespace SiteInspect.Application.Features.CorrectiveActions.Commands.SubmitCorrectiveActionResponse;

public sealed class SubmitCorrectiveActionResponseValidator
    : AbstractValidator<SubmitCorrectiveActionResponseCommand>
{
    public SubmitCorrectiveActionResponseValidator()
    {
        RuleFor(command => command.CorrectiveActionId).NotEmpty();
        RuleFor(command => command.ResolutionNotes).NotEmpty().MaximumLength(2000);
        RuleFor(command => command.RowVersion)
            .Must(RowVersionToken.IsValid)
            .WithMessage("A valid row version is required.");
    }
}

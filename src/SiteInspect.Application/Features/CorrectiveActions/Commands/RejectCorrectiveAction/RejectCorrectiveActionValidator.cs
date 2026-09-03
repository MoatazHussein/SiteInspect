using FluentValidation;
using SiteInspect.Application.Common.Persistence;

namespace SiteInspect.Application.Features.CorrectiveActions.Commands.RejectCorrectiveAction;

public sealed class RejectCorrectiveActionValidator : AbstractValidator<RejectCorrectiveActionCommand>
{
    public RejectCorrectiveActionValidator()
    {
        RuleFor(command => command.CorrectiveActionId).NotEmpty();
        RuleFor(command => command.Reason).NotEmpty().MaximumLength(2000);
        RuleFor(command => command.RowVersion)
            .Must(RowVersionToken.IsValid)
            .WithMessage("A valid row version is required.");
    }
}

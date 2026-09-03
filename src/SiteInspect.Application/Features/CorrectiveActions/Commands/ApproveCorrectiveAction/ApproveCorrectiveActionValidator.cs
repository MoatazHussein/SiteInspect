using FluentValidation;
using SiteInspect.Application.Common.Persistence;

namespace SiteInspect.Application.Features.CorrectiveActions.Commands.ApproveCorrectiveAction;

public sealed class ApproveCorrectiveActionValidator : AbstractValidator<ApproveCorrectiveActionCommand>
{
    public ApproveCorrectiveActionValidator()
    {
        RuleFor(command => command.CorrectiveActionId).NotEmpty();
        RuleFor(command => command.RowVersion)
            .Must(RowVersionToken.IsValid)
            .WithMessage("A valid row version is required.");
    }
}

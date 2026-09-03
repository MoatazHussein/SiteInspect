using FluentValidation;
using SiteInspect.Application.Common.Persistence;

namespace SiteInspect.Application.Features.CorrectiveActions.Commands.CreateCorrectiveAction;

public sealed class CreateCorrectiveActionValidator : AbstractValidator<CreateCorrectiveActionCommand>
{
    public CreateCorrectiveActionValidator(TimeProvider timeProvider)
    {
        RuleFor(command => command.InspectionId).NotEmpty();
        RuleFor(command => command.ObservationId).NotEmpty();
        RuleFor(command => command.ContractorId).NotEmpty();
        RuleFor(command => command.Description).NotEmpty().MaximumLength(2000);
        RuleFor(command => command.RowVersion).Must(RowVersionToken.IsValid)
            .WithMessage("A valid inspection row version is required.");
        RuleFor(command => command.DueAtUtc)
            .Must(dueAt => dueAt > timeProvider.GetUtcNow())
            .WithMessage("The due date must be in the future.");
    }
}


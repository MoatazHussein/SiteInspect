using FluentValidation;
using SiteInspect.Application.Common.Persistence;

namespace SiteInspect.Application.Features.Inspections.Commands.CancelInspection;

public sealed class CancelInspectionValidator : AbstractValidator<CancelInspectionCommand>
{
    public CancelInspectionValidator()
    {
        RuleFor(command => command.InspectionId).NotEmpty();
        RuleFor(command => command.Reason)
            .NotEmpty()
            .MaximumLength(1000);
        RuleFor(command => command.RowVersion)
            .Must(RowVersionToken.IsValid)
            .WithMessage("A valid row version is required.");
    }
}

using FluentValidation;
using SiteInspect.Application.Common.Persistence;

namespace SiteInspect.Application.Features.Inspections.Commands.CompleteInspection;

public sealed class CompleteInspectionValidator : AbstractValidator<CompleteInspectionCommand>
{
    public CompleteInspectionValidator()
    {
        RuleFor(command => command.InspectionId).NotEmpty();
        RuleFor(command => command.RowVersion)
            .Must(RowVersionToken.IsValid)
            .WithMessage("A valid row version is required.");
    }
}

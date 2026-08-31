using FluentValidation;
using SiteInspect.Application.Common.Persistence;

namespace SiteInspect.Application.Features.Inspections.Commands.StartInspection;

public sealed class StartInspectionValidator : AbstractValidator<StartInspectionCommand>
{
    public StartInspectionValidator()
    {
        RuleFor(command => command.InspectionId).NotEmpty();
        RuleFor(command => command.RowVersion)
            .Must(RowVersionToken.IsValid)
            .WithMessage("A valid row version is required.");
    }
}

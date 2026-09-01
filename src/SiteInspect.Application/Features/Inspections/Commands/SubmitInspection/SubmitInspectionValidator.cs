using FluentValidation;
using SiteInspect.Application.Common.Persistence;

namespace SiteInspect.Application.Features.Inspections.Commands.SubmitInspection;

public sealed class SubmitInspectionValidator : AbstractValidator<SubmitInspectionCommand>
{
    public SubmitInspectionValidator()
    {
        RuleFor(command => command.InspectionId).NotEmpty();
        RuleFor(command => command.RowVersion)
            .Must(RowVersionToken.IsValid)
            .WithMessage("A valid row version is required.");
    }
}

using FluentValidation;
using SiteInspect.Application.Common.Persistence;

namespace SiteInspect.Application.Features.Inspections.Commands.ReassignInspection;

public sealed class ReassignInspectionValidator : AbstractValidator<ReassignInspectionCommand>
{
    public ReassignInspectionValidator()
    {
        RuleFor(command => command.InspectionId).NotEmpty();
        RuleFor(command => command.InspectorId).NotEmpty();
        RuleFor(command => command.RowVersion)
            .Must(RowVersionToken.IsValid)
            .WithMessage("A valid row version is required.");
    }
}

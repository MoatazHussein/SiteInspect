using FluentValidation;

namespace SiteInspect.Application.Features.Inspections.Commands.CreateInspection;

public sealed class CreateInspectionValidator : AbstractValidator<CreateInspectionCommand>
{
    public CreateInspectionValidator(TimeProvider timeProvider)
    {
        RuleFor(command => command.ProjectId).NotEmpty();
        RuleFor(command => command.LocationId).NotEmpty();
        RuleFor(command => command.TemplateId).NotEmpty();
        RuleFor(command => command.InspectorId).NotEmpty();
        RuleFor(command => command.DueAtUtc)
            .GreaterThan(timeProvider.GetUtcNow())
            .WithMessage("The due date must be in the future.");
    }
}

using FluentValidation;
using SiteInspect.Application.Common.Persistence;

namespace SiteInspect.Application.Features.Inspections.Commands.SaveInspectionDraft;

public sealed class SaveInspectionDraftValidator : AbstractValidator<SaveInspectionDraftCommand>
{
    public SaveInspectionDraftValidator()
    {
        RuleFor(command => command.InspectionId).NotEmpty();
        RuleFor(command => command.RowVersion)
            .Must(RowVersionToken.IsValid)
            .WithMessage("A valid row version is required.");
        RuleFor(command => command.Observations).NotEmpty();
        RuleForEach(command => command.Observations)
            .SetValidator(new SaveInspectionObservationDraftValidator());
        RuleFor(command => command.Observations)
            .Must(observations =>
                observations is not null &&
                observations.Select(item => item.ObservationId).Distinct().Count() == observations.Count)
            .WithMessage("An observation can only appear once in a draft request.");
    }
}

using FluentValidation;

namespace SiteInspect.Application.Features.Inspections.Commands.SaveInspectionDraft;

public sealed class SaveInspectionObservationDraftValidator
    : AbstractValidator<SaveInspectionObservationDraft>
{
    public SaveInspectionObservationDraftValidator()
    {
        RuleFor(draft => draft.ObservationId).NotEmpty();
        RuleFor(draft => draft.Notes).MaximumLength(2000);
    }
}

using FluentValidation;
using SiteInspect.Application.Common.Persistence;

namespace SiteInspect.Application.Features.Inspections.Commands.UploadInspectionAttachment;

public sealed class UploadInspectionAttachmentValidator
    : AbstractValidator<UploadInspectionAttachmentCommand>
{
    private const long MaximumFileLength = 10 * 1024 * 1024;
    private static readonly string[] AllowedContentTypes =
        ["image/jpeg", "image/png", "image/webp"];

    public UploadInspectionAttachmentValidator()
    {
        RuleFor(command => command.InspectionId).NotEmpty();
        RuleFor(command => command.ObservationId).NotEmpty();
        RuleFor(command => command.RowVersion)
            .Must(RowVersionToken.IsValid)
            .WithMessage("A valid row version is required.");
        RuleFor(command => command.FileName).NotEmpty().MaximumLength(255);
        RuleFor(command => command.ContentType)
            .Must(contentType => AllowedContentTypes.Contains(contentType, StringComparer.OrdinalIgnoreCase))
            .WithMessage("Only JPEG, PNG, and WebP images are supported.");
        RuleFor(command => command.Length)
            .InclusiveBetween(1, MaximumFileLength)
            .WithMessage("The image must be no larger than 10 MB.");
        RuleFor(command => command.Content).NotNull();
    }
}

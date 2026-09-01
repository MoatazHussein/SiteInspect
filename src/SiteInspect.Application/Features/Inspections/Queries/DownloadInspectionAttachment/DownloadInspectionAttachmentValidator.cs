using FluentValidation;

namespace SiteInspect.Application.Features.Inspections.Queries.DownloadInspectionAttachment;

public sealed class DownloadInspectionAttachmentValidator
    : AbstractValidator<DownloadInspectionAttachmentQuery>
{
    public DownloadInspectionAttachmentValidator()
    {
        RuleFor(query => query.InspectionId).NotEmpty();
        RuleFor(query => query.AttachmentId).NotEmpty();
    }
}

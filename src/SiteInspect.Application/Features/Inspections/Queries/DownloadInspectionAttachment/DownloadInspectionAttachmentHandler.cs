using MediatR;
using SiteInspect.Application.Common.Abstractions.Identity;
using SiteInspect.Application.Common.Abstractions.Storage;
using SiteInspect.Application.Common.Authorization;
using SiteInspect.Application.Common.Errors;
using SiteInspect.Application.Common.Results;
using SiteInspect.Application.Features.Inspections.Common;

namespace SiteInspect.Application.Features.Inspections.Queries.DownloadInspectionAttachment;

public sealed class DownloadInspectionAttachmentHandler(
    ICurrentUser currentUser,
    IInspectionRepository inspectionRepository,
    IInspectionAttachmentStorage attachmentStorage)
    : IRequestHandler<DownloadInspectionAttachmentQuery, Result<InspectionAttachmentDownload>>
{
    public async Task<Result<InspectionAttachmentDownload>> Handle(
        DownloadInspectionAttachmentQuery query,
        CancellationToken cancellationToken)
    {
        var inspection = await inspectionRepository.GetWithAttachmentsAsync(
            query.InspectionId,
            cancellationToken);

        if (inspection is null)
        {
            return Result<InspectionAttachmentDownload>.Failure(
                InspectionErrors.NotFound(query.InspectionId));
        }

        var canRead = currentUser.IsInRole(RoleNames.Manager) ||
            (currentUser.IsInRole(RoleNames.Inspector) &&
             currentUser.UserId == inspection.AssignedInspectorId);
        if (!canRead)
        {
            return Result<InspectionAttachmentDownload>.Failure(AuthorizationErrors.Forbidden);
        }

        var attachment = inspection.Observations
            .SelectMany(observation => observation.Attachments)
            .SingleOrDefault(item => item.Id == query.AttachmentId);
        if (attachment is null)
        {
            return Result<InspectionAttachmentDownload>.Failure(
                InspectionErrors.AttachmentNotFound(query.AttachmentId));
        }

        var content = await attachmentStorage.OpenReadAsync(
            attachment.StoredFileName,
            cancellationToken);
        if (content is null)
        {
            return Result<InspectionAttachmentDownload>.Failure(
                InspectionErrors.AttachmentNotFound(query.AttachmentId));
        }

        return Result<InspectionAttachmentDownload>.Success(new InspectionAttachmentDownload(
            content,
            attachment.ContentType,
            attachment.OriginalFileName));
    }
}

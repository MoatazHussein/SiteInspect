using SiteInspect.Application.Common.Messaging;

namespace SiteInspect.Application.Features.Inspections.Queries.DownloadInspectionAttachment;

public sealed record DownloadInspectionAttachmentQuery(
    Guid InspectionId,
    Guid AttachmentId) : IQuery<InspectionAttachmentDownload>;

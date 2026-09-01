namespace SiteInspect.Application.Features.Inspections.Queries.DownloadInspectionAttachment;

public sealed record InspectionAttachmentDownload(
    Stream Content,
    string ContentType,
    string FileName);

namespace SiteInspect.Application.Features.Inspections.Commands.UploadInspectionAttachment;

public sealed record UploadInspectionAttachmentResponse(
    Guid Id,
    Guid ObservationId,
    string FileName,
    string ContentType,
    long Length,
    DateTimeOffset UploadedAtUtc,
    string RowVersion);

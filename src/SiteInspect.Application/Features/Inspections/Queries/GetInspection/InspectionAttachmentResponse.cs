namespace SiteInspect.Application.Features.Inspections.Queries.GetInspection;

public sealed record InspectionAttachmentResponse(
    Guid Id,
    string FileName,
    string ContentType,
    long Length,
    DateTimeOffset UploadedAtUtc);

using SiteInspect.Domain.Common.Entities;

namespace SiteInspect.Domain.Inspections.InspectionAggregate;

public sealed class InspectionAttachment : AuditableEntity
{
    private InspectionAttachment()
    {
    }

    internal InspectionAttachment(
        Guid id,
        Guid observationId,
        string originalFileName,
        string storedFileName,
        string contentType,
        long length)
    {
        Id = id;
        ObservationId = observationId;
        OriginalFileName = originalFileName;
        StoredFileName = storedFileName;
        ContentType = contentType;
        Length = length;
    }

    public Guid ObservationId { get; private set; }

    public string OriginalFileName { get; private set; } = string.Empty;

    public string StoredFileName { get; private set; } = string.Empty;

    public string ContentType { get; private set; } = string.Empty;

    public long Length { get; private set; }
}

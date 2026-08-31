namespace SiteInspect.Domain.Common.Entities;

public abstract class ConcurrentAuditableEntity : AuditableEntity
{
    public byte[] RowVersion { get; private set; } = [];
}

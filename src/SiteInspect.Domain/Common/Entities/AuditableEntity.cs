namespace SiteInspect.Domain.Common.Entities;

public abstract class AuditableEntity : BaseEntity
{
    public DateTimeOffset CreatedAtUtc { get; private set; }

    public Guid? CreatedBy { get; private set; }

    public DateTimeOffset? LastModifiedAtUtc { get; private set; }

    public Guid? LastModifiedBy { get; private set; }

    internal void SetCreatedAudit(DateTimeOffset occurredAtUtc, Guid? actorId)
    {
        CreatedAtUtc = occurredAtUtc;
        CreatedBy = actorId;
    }

    internal void SetModifiedAudit(DateTimeOffset occurredAtUtc, Guid? actorId)
    {
        LastModifiedAtUtc = occurredAtUtc;
        LastModifiedBy = actorId;
    }
}

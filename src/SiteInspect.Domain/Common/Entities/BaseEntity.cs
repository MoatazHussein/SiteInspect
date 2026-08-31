namespace SiteInspect.Domain.Common.Entities;

public abstract class BaseEntity
{
    public Guid Id { get; protected set; } = Guid.CreateVersion7();
}

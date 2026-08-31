using SiteInspect.Domain.Common.Entities;

namespace SiteInspect.Domain.Projects.ProjectAggregate;

public sealed class ProjectLocation : AuditableEntity
{
    private ProjectLocation()
    {
    }

    internal ProjectLocation(Guid id, Guid projectId, string name, Guid? parentLocationId)
    {
        Id = id;
        ProjectId = projectId;
        Name = name;
        ParentLocationId = parentLocationId;
    }

    public Guid ProjectId { get; private set; }

    public string Name { get; private set; } = string.Empty;

    public Guid? ParentLocationId { get; private set; }
}

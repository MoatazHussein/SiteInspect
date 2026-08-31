using SiteInspect.Domain.Common.Entities;

namespace SiteInspect.Domain.Projects.ProjectAggregate;

public sealed class Project : AuditableEntity
{
    private Project()
    {
    }

    internal Project(Guid id, string name, string code, bool isActive)
    {
        Id = id;
        Name = name;
        Code = code;
        IsActive = isActive;
    }

    public string Name { get; private set; } = string.Empty;

    public string Code { get; private set; } = string.Empty;

    public bool IsActive { get; private set; }
}

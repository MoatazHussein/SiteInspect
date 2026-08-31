using SiteInspect.Domain.Common.Entities;

namespace SiteInspect.Domain.Inspections.InspectionTemplateAggregate;

public sealed class InspectionTemplate : AuditableEntity
{
    private readonly List<InspectionTemplateItem> items = [];

    private InspectionTemplate()
    {
    }

    internal InspectionTemplate(
        Guid id,
        string name,
        int version,
        bool isActive,
        IEnumerable<InspectionTemplateItem> templateItems)
    {
        Id = id;
        Name = name;
        Version = version;
        IsActive = isActive;
        items.AddRange(templateItems);
    }

    public string Name { get; private set; } = string.Empty;

    public int Version { get; private set; }

    public bool IsActive { get; private set; }

    public IReadOnlyCollection<InspectionTemplateItem> Items => items;
}

using SiteInspect.Domain.Common.Entities;
using SiteInspect.Domain.Inspections;

namespace SiteInspect.Domain.Inspections.InspectionTemplateAggregate;

public sealed class InspectionTemplateItem : BaseEntity
{
    private InspectionTemplateItem()
    {
    }

    internal InspectionTemplateItem(
        Guid id,
        Guid inspectionTemplateId,
        string sectionName,
        string question,
        int displayOrder,
        bool isRequired,
        Severity defaultSeverity)
    {
        Id = id;
        InspectionTemplateId = inspectionTemplateId;
        SectionName = sectionName;
        Question = question;
        DisplayOrder = displayOrder;
        IsRequired = isRequired;
        DefaultSeverity = defaultSeverity;
    }

    public Guid InspectionTemplateId { get; private set; }

    public string SectionName { get; private set; } = string.Empty;

    public string Question { get; private set; } = string.Empty;

    public int DisplayOrder { get; private set; }

    public bool IsRequired { get; private set; }

    public Severity DefaultSeverity { get; private set; }
}

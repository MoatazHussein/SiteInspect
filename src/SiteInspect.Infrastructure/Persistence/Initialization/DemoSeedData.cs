using SiteInspect.Domain.Inspections;
using SiteInspect.Domain.Inspections.InspectionTemplateAggregate;

namespace SiteInspect.Infrastructure.Persistence.Initialization;

internal static class DemoSeedData
{
    internal static readonly Guid ManagerUserId = Guid.Parse("019c9a10-0001-7000-8000-000000000001");
    internal static readonly Guid InspectorUserId = Guid.Parse("019c9a10-0001-7000-8000-000000000002");
    internal static readonly Guid SecondInspectorUserId = Guid.Parse("019c9a10-0001-7000-8000-000000000004");
    internal static readonly Guid ContractorUserId = Guid.Parse("019c9a10-0001-7000-8000-000000000003");
    internal static readonly Guid ProjectId = Guid.Parse("019c9a10-0010-7000-8000-000000000001");
    internal static readonly Guid BuildingId = Guid.Parse("019c9a10-0011-7000-8000-000000000001");
    internal static readonly Guid FloorId = Guid.Parse("019c9a10-0011-7000-8000-000000000002");
    internal static readonly Guid CorridorId = Guid.Parse("019c9a10-0011-7000-8000-000000000003");
    internal static readonly Guid TemplateId = Guid.Parse("019c9a10-0020-7000-8000-000000000001");
    internal static readonly Guid AssignedInspectionId = Guid.Parse("019c9a10-0030-7000-8000-000000000001");
    internal static readonly Guid CompletedInspectionId = Guid.Parse("019c9a10-0030-7000-8000-000000000002");
    private static readonly (string Section, string Question, bool IsRequired, Severity Severity)[] TemplateItems =
    [
        new("Electrical", "Emergency lighting is installed and operational.", true, Severity.Critical),
        new("Electrical", "Electrical panels are closed, labelled, and unobstructed.", true, Severity.High),
        new("Electrical", "Temporary wiring is protected from physical damage.", true, Severity.High),
        new("Fire Safety", "Fire extinguishers are present and within inspection date.", true, Severity.Critical),
        new("Fire Safety", "Escape routes are illuminated and free from obstruction.", true, Severity.Critical),
        new("Access", "Corridor access is clear and walking surfaces are even.", true, Severity.Medium),
        new("Access", "Guardrails and edge protection are secure.", true, Severity.High),
        new("Housekeeping", "Combustible waste has been removed from the work area.", true, Severity.Medium),
        new("Housekeeping", "Materials are stacked securely without blocking access.", true, Severity.Low),
        new("Documentation", "Required safety signage is visible and legible.", false, Severity.Low),
    ];

    internal static InspectionTemplateItem[] CreateTemplateItems() =>
        TemplateItems.Select((item, index) => new InspectionTemplateItem(
            CreateStableTemplateItemId(index + 1),
            TemplateId,
            item.Section,
            item.Question,
            index + 1,
            item.IsRequired,
            item.Severity)).ToArray();

    private static Guid CreateStableTemplateItemId(int itemNumber) =>
        Guid.Parse($"019c9a10-0021-7000-8000-{itemNumber:000000000000}");
}

namespace SiteInspect.Application.Features.Inspections.Queries.GetInspectionManagementOptions;

public sealed record GetInspectionManagementOptionsResponse(
    IReadOnlyCollection<ManagementProjectOption> Projects,
    IReadOnlyCollection<ManagementLocationOption> Locations,
    IReadOnlyCollection<ManagementTemplateOption> Templates,
    IReadOnlyCollection<ManagementInspectorOption> Inspectors);

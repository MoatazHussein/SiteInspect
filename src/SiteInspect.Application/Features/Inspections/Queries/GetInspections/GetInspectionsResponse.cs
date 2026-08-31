namespace SiteInspect.Application.Features.Inspections.Queries.GetInspections;

public sealed record GetInspectionsResponse(
    IReadOnlyCollection<InspectionListItem> Items,
    int Page,
    int PageSize,
    int TotalCount,
    int TotalPages);

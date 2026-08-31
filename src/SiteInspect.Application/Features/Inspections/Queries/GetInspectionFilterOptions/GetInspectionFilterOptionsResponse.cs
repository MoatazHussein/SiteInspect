namespace SiteInspect.Application.Features.Inspections.Queries.GetInspectionFilterOptions;

public sealed record GetInspectionFilterOptionsResponse(
    IReadOnlyCollection<FilterOption<Guid>> Projects,
    IReadOnlyCollection<FilterOption<Guid>> Locations,
    IReadOnlyCollection<FilterOption<Guid>> Inspectors,
    IReadOnlyCollection<FilterOption<string>> Statuses);

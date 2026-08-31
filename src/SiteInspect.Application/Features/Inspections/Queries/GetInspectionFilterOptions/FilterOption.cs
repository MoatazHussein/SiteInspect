namespace SiteInspect.Application.Features.Inspections.Queries.GetInspectionFilterOptions;

public sealed record FilterOption<T>(T Value, string Label);

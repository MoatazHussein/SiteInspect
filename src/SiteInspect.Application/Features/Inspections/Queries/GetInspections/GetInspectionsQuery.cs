using SiteInspect.Domain.Inspections.InspectionAggregate;
using SiteInspect.Application.Common.Messaging;

namespace SiteInspect.Application.Features.Inspections.Queries.GetInspections;

public sealed record GetInspectionsQuery(
    int Page = 1,
    int PageSize = 20,
    string? Search = null,
    Guid? ProjectId = null,
    Guid? LocationId = null,
    Guid? InspectorId = null,
    InspectionStatus? Status = null,
    DateTimeOffset? DueFromUtc = null,
    DateTimeOffset? DueToUtc = null) : IQuery<GetInspectionsResponse>;

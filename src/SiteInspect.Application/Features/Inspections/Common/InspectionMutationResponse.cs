using SiteInspect.Domain.Inspections.InspectionAggregate;

namespace SiteInspect.Application.Features.Inspections.Common;

public sealed record InspectionMutationResponse(
    Guid Id,
    InspectionStatus Status,
    Guid AssignedInspectorId,
    string RowVersion);

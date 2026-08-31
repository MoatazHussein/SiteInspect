using SiteInspect.Domain.Inspections.InspectionAggregate;

namespace SiteInspect.Application.Features.Inspections.Commands.CreateInspection;

public sealed record CreateInspectionResponse(
    Guid Id,
    string Number,
    InspectionStatus Status,
    string RowVersion);

using SiteInspect.Domain.Inspections.InspectionAggregate;

namespace SiteInspect.Application.Features.CorrectiveActions.Commands.CreateCorrectiveAction;

public sealed record CreateCorrectiveActionResponse(Guid Id, InspectionStatus InspectionStatus, string RowVersion);

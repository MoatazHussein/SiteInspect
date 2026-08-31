using SiteInspect.Application.Common.Messaging;
using SiteInspect.Application.Features.Inspections.Common;

namespace SiteInspect.Application.Features.Inspections.Commands.CancelInspection;

public sealed record CancelInspectionCommand(
    Guid InspectionId,
    string Reason,
    string RowVersion) : ICommand<InspectionMutationResponse>;

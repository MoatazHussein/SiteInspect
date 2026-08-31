using SiteInspect.Application.Common.Messaging;
using SiteInspect.Application.Features.Inspections.Common;

namespace SiteInspect.Application.Features.Inspections.Commands.ReassignInspection;

public sealed record ReassignInspectionCommand(
    Guid InspectionId,
    Guid InspectorId,
    string RowVersion) : ICommand<InspectionMutationResponse>;

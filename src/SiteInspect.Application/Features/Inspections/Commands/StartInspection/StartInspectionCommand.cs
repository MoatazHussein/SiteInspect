using SiteInspect.Application.Common.Messaging;
using SiteInspect.Application.Features.Inspections.Common;

namespace SiteInspect.Application.Features.Inspections.Commands.StartInspection;

public sealed record StartInspectionCommand(
    Guid InspectionId,
    string RowVersion) : ICommand<InspectionMutationResponse>;

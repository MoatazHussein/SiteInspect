using SiteInspect.Application.Common.Messaging;
using SiteInspect.Application.Features.Inspections.Common;

namespace SiteInspect.Application.Features.Inspections.Commands.CompleteInspection;

public sealed record CompleteInspectionCommand(
    Guid InspectionId,
    string RowVersion) : ICommand<InspectionMutationResponse>;

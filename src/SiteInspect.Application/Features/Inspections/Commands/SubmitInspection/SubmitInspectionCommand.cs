using SiteInspect.Application.Common.Messaging;
using SiteInspect.Application.Features.Inspections.Common;

namespace SiteInspect.Application.Features.Inspections.Commands.SubmitInspection;

public sealed record SubmitInspectionCommand(
    Guid InspectionId,
    string RowVersion) : ICommand<InspectionMutationResponse>;

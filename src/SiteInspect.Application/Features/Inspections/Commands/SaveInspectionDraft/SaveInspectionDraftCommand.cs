using SiteInspect.Application.Common.Messaging;
using SiteInspect.Application.Features.Inspections.Common;

namespace SiteInspect.Application.Features.Inspections.Commands.SaveInspectionDraft;

public sealed record SaveInspectionDraftCommand(
    Guid InspectionId,
    string RowVersion,
    IReadOnlyCollection<SaveInspectionObservationDraft> Observations)
    : ICommand<InspectionMutationResponse>;

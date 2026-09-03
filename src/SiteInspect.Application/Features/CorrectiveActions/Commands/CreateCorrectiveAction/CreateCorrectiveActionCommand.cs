using SiteInspect.Application.Common.Messaging;

namespace SiteInspect.Application.Features.CorrectiveActions.Commands.CreateCorrectiveAction;

public sealed record CreateCorrectiveActionCommand(
    Guid InspectionId,
    Guid ObservationId,
    Guid ContractorId,
    string Description,
    DateTimeOffset DueAtUtc,
    string RowVersion) : ICommand<CreateCorrectiveActionResponse>;


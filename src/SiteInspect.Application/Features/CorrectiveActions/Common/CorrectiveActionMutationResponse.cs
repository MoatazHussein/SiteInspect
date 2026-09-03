using SiteInspect.Domain.CorrectiveActions.CorrectiveActionAggregate;

namespace SiteInspect.Application.Features.CorrectiveActions.Common;

public sealed record CorrectiveActionMutationResponse(
    Guid Id,
    CorrectiveActionStatus Status,
    string RowVersion);

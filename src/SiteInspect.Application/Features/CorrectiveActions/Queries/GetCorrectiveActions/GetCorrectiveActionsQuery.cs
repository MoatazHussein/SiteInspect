using SiteInspect.Application.Common.Messaging;
using SiteInspect.Application.Features.CorrectiveActions.Common;

namespace SiteInspect.Application.Features.CorrectiveActions.Queries.GetCorrectiveActions;

public sealed record GetCorrectiveActionsQuery(Guid InspectionId) : IQuery<IReadOnlyCollection<CorrectiveActionResponse>>;


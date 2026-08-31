using SiteInspect.Application.Common.Messaging;

namespace SiteInspect.Application.Features.Authentication.Queries.GetCurrentUser;

public sealed record GetCurrentUserQuery(Guid UserId) : IQuery<GetCurrentUserResponse>;

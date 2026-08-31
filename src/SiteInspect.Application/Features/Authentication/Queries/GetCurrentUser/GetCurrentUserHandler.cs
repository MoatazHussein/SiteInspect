using MediatR;
using SiteInspect.Application.Common.Results;
using SiteInspect.Application.Features.Authentication.Common;

namespace SiteInspect.Application.Features.Authentication.Queries.GetCurrentUser;

public sealed class GetCurrentUserHandler(IUserSessionService sessionService)
    : IRequestHandler<GetCurrentUserQuery, Result<GetCurrentUserResponse>>
{
    public async Task<Result<GetCurrentUserResponse>> Handle(
        GetCurrentUserQuery query,
        CancellationToken cancellationToken)
    {
        var result = await sessionService.GetCurrentUserAsync(query.UserId, cancellationToken);
        if (result.IsFailure)
        {
            return Result<GetCurrentUserResponse>.Failure(result.Errors);
        }

        var user = result.Value;
        return Result<GetCurrentUserResponse>.Success(new GetCurrentUserResponse(
            user.Id,
            user.Email,
            user.DisplayName,
            user.Roles));
    }
}

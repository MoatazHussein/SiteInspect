using SiteInspect.Application.Common.Abstractions.Identity;

namespace SiteInspect.Infrastructure.Identity.Services;

internal sealed class NullCurrentUser : ICurrentUser
{
    public Guid? UserId => null;

    public bool IsAuthenticated => false;

    public bool IsInRole(string role) => false;
}

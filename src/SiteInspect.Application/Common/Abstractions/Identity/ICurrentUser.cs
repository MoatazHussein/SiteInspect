namespace SiteInspect.Application.Common.Abstractions.Identity;

public interface ICurrentUser
{
    Guid? UserId { get; }

    bool IsAuthenticated { get; }

    bool IsInRole(string role);
}

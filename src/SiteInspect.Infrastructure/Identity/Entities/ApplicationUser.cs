using Microsoft.AspNetCore.Identity;

namespace SiteInspect.Infrastructure.Identity.Entities;

public sealed class ApplicationUser : IdentityUser<Guid>
{
    public string DisplayName { get; set; } = string.Empty;
}

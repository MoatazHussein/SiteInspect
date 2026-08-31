using Microsoft.AspNetCore.Identity;
using SiteInspect.Application.Common.Authorization;
using SiteInspect.Application.Features.Inspections.Common;
using SiteInspect.Infrastructure.Identity.Entities;

namespace SiteInspect.Infrastructure.Identity.Services;

internal sealed class InspectorDirectory(UserManager<ApplicationUser> userManager) : IInspectorDirectory
{
    public async Task<bool> ExistsAsync(Guid inspectorId, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var inspector = await userManager.FindByIdAsync(inspectorId.ToString());
        return inspector is not null && await userManager.IsInRoleAsync(inspector, RoleNames.Inspector);
    }
}

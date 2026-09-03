using Microsoft.AspNetCore.Identity;
using SiteInspect.Application.Common.Authorization;
using SiteInspect.Infrastructure.Identity.Entities;
using SiteInspect.Application.Features.CorrectiveActions.Common;


namespace SiteInspect.Infrastructure.Identity.Services;

internal sealed class ContractorDirectory(UserManager<ApplicationUser> userManager) : IContractorDirectory
{
    public async Task<bool> ExistsAsync(Guid contractorId, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var user = await userManager.FindByIdAsync(contractorId.ToString());
        return user is not null && await userManager.IsInRoleAsync(user, RoleNames.Contractor);
    }
}


using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using SiteInspect.Application.Common.Authorization;
using SiteInspect.Infrastructure.Identity.Entities;

namespace SiteInspect.Infrastructure.Persistence.Initialization.Steps;

internal sealed class IdentitySeedStep(
    RoleManager<IdentityRole<Guid>> roleManager,
    UserManager<ApplicationUser> userManager,
    IConfiguration configuration) : ISeedStep
{
    public string Name => "Roles and users";
    public int Order => SeedStepOrder.Identity;

    public async Task SeedAsync(CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var passwords = ReadPasswords();
        await SeedRolesAndUsersAsync(passwords);
    }

    private (string Manager, string Inspector, string Contractor) ReadPasswords()
    {
        var manager = configuration["DemoData:Users:Manager:Password"];
        var inspector = configuration["DemoData:Users:Inspector:Password"];
        var contractor = configuration["DemoData:Users:Contractor:Password"];

        if (string.IsNullOrWhiteSpace(manager) ||
            string.IsNullOrWhiteSpace(inspector) ||
            string.IsNullOrWhiteSpace(contractor))
        {
            throw new InvalidOperationException(
                "Demo data is enabled. Configure a separate Manager, Inspector, and Contractor password outside source control.");
        }

        return (manager, inspector, contractor);
    }

    private async Task SeedRolesAndUsersAsync(
        (string Manager, string Inspector, string Contractor) passwords)
    {
        foreach (var roleName in RoleNames.All)
        {
            if (!await roleManager.RoleExistsAsync(roleName))
            {
                EnsureSucceeded(await roleManager.CreateAsync(new IdentityRole<Guid>(roleName)), $"create role {roleName}");
            }
        }

        await EnsureUserAsync(
            DemoSeedData.ManagerUserId,
            "manager@siteinspect.demo",
            "Khaled Haddad",
            RoleNames.Manager,
            passwords.Manager);

        await EnsureUserAsync(
            DemoSeedData.InspectorUserId,
            "inspector@siteinspect.demo",
            "Ahmed Khalil",
            RoleNames.Inspector,
            passwords.Inspector);

        await EnsureUserAsync(
            DemoSeedData.SecondInspectorUserId,
            "inspector2@siteinspect.demo",
            "Nour Saad",
            RoleNames.Inspector,
            passwords.Inspector);

        await EnsureUserAsync(
            DemoSeedData.ContractorUserId,
            "contractor@siteinspect.demo",
            "Rami Nasser",
            RoleNames.Contractor,
            passwords.Contractor);
    }

    private async Task EnsureUserAsync(
        Guid id,
        string email,
        string displayName,
        string role,
        string password)
    {
        var user = await userManager.FindByEmailAsync(email);

        if (user is null)
        {
            user = new ApplicationUser
            {
                Id = id,
                UserName = email,
                Email = email,
                EmailConfirmed = true,
                DisplayName = displayName,
            };

            EnsureSucceeded(await userManager.CreateAsync(user, password), $"create demo user {email}");
        }
        else if (!string.Equals(user.DisplayName, displayName, StringComparison.Ordinal))
        {
            user.DisplayName = displayName;
            EnsureSucceeded(await userManager.UpdateAsync(user), $"update demo user {email}");
        }

        if (!await userManager.CheckPasswordAsync(user, password))
        {
            var resetToken = await userManager.GeneratePasswordResetTokenAsync(user);
            EnsureSucceeded(
                await userManager.ResetPasswordAsync(user, resetToken, password),
                $"synchronize demo user password for {email}");
        }

        if (!await userManager.IsInRoleAsync(user, role))
        {
            EnsureSucceeded(await userManager.AddToRoleAsync(user, role), $"assign {email} to {role}");
        }
    }

    private static void EnsureSucceeded(IdentityResult result, string operation)
    {
        if (result.Succeeded)
        {
            return;
        }

        var errors = string.Join(", ", result.Errors.Select(error => error.Description));
        throw new InvalidOperationException($"Unable to {operation}: {errors}");
    }
}

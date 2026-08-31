using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using SiteInspect.Application.Common.Authorization;
using SiteInspect.Application.Common.Numbering;
using SiteInspect.Domain.Common.Numbering;
using SiteInspect.Domain.Inspections;
using SiteInspect.Domain.Inspections.InspectionAggregate;
using SiteInspect.Domain.Inspections.InspectionTemplateAggregate;
using SiteInspect.Domain.Projects.ProjectAggregate;
using SiteInspect.Infrastructure.Identity.Entities;
using SiteInspect.Infrastructure.Persistence;

namespace SiteInspect.Infrastructure.Persistence.Initialization;

internal sealed partial class DatabaseSeeder(
    ApplicationDbContext dbContext,
    RoleManager<IdentityRole<Guid>> roleManager,
    UserManager<ApplicationUser> userManager,
    IConfiguration configuration,
    TimeProvider timeProvider,
    ILogger<DatabaseSeeder> logger) : IDatabaseSeeder
{
    private static readonly Guid ManagerUserId = Guid.Parse("019c9a10-0001-7000-8000-000000000001");
    private static readonly Guid InspectorUserId = Guid.Parse("019c9a10-0001-7000-8000-000000000002");
    private static readonly Guid SecondInspectorUserId = Guid.Parse("019c9a10-0001-7000-8000-000000000004");
    private static readonly Guid ContractorUserId = Guid.Parse("019c9a10-0001-7000-8000-000000000003");
    private static readonly Guid ProjectId = Guid.Parse("019c9a10-0010-7000-8000-000000000001");
    private static readonly Guid BuildingId = Guid.Parse("019c9a10-0011-7000-8000-000000000001");
    private static readonly Guid FloorId = Guid.Parse("019c9a10-0011-7000-8000-000000000002");
    private static readonly Guid CorridorId = Guid.Parse("019c9a10-0011-7000-8000-000000000003");
    private static readonly Guid TemplateId = Guid.Parse("019c9a10-0020-7000-8000-000000000001");
    private static readonly Guid AssignedInspectionId = Guid.Parse("019c9a10-0030-7000-8000-000000000001");
    private static readonly Guid CompletedInspectionId = Guid.Parse("019c9a10-0030-7000-8000-000000000002");
    private static readonly (string Section, string Question, bool IsRequired, Severity Severity)[] TemplateItems =
    [
        new("Electrical", "Emergency lighting is installed and operational.", true, Severity.Critical),
        new("Electrical", "Electrical panels are closed, labelled, and unobstructed.", true, Severity.High),
        new("Electrical", "Temporary wiring is protected from physical damage.", true, Severity.High),
        new("Fire Safety", "Fire extinguishers are present and within inspection date.", true, Severity.Critical),
        new("Fire Safety", "Escape routes are illuminated and free from obstruction.", true, Severity.Critical),
        new("Access", "Corridor access is clear and walking surfaces are even.", true, Severity.Medium),
        new("Access", "Guardrails and edge protection are secure.", true, Severity.High),
        new("Housekeeping", "Combustible waste has been removed from the work area.", true, Severity.Medium),
        new("Housekeeping", "Materials are stacked securely without blocking access.", true, Severity.Low),
        new("Documentation", "Required safety signage is visible and legible.", false, Severity.Low),
    ];

    public async Task SeedAsync(CancellationToken cancellationToken)
    {
        var passwords = ReadPasswords();
        await SeedRolesAndUsersAsync(passwords);
        await SeedDomainDataAsync(cancellationToken);
        LogDemoDataReady(logger);
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
            ManagerUserId,
            "manager@siteinspect.demo",
            "Khaled Haddad",
            RoleNames.Manager,
            passwords.Manager);

        await EnsureUserAsync(
            InspectorUserId,
            "inspector@siteinspect.demo",
            "Ahmed Khalil",
            RoleNames.Inspector,
            passwords.Inspector);

        await EnsureUserAsync(
            SecondInspectorUserId,
            "inspector2@siteinspect.demo",
            "Nour Saad",
            RoleNames.Inspector,
            passwords.Inspector);

        await EnsureUserAsync(
            ContractorUserId,
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

    private async Task SeedDomainDataAsync(CancellationToken cancellationToken)
    {
        var now = timeProvider.GetUtcNow();
        var project = new Project(ProjectId, "Harbor View Apartments", "APT-001", true);
        var locations = new ProjectLocation[]
        {
            new ProjectLocation(BuildingId, ProjectId, "Building A", null),
            new ProjectLocation(FloorId, ProjectId, "Floor 5", BuildingId),
            new ProjectLocation(CorridorId, ProjectId, "Eastern Corridor", FloorId),
        };

        var templateItems = TemplateItems.Select((item, index) => new InspectionTemplateItem(
            CreateStableTemplateItemId(index + 1),
            TemplateId,
            item.Section,
            item.Question,
            index + 1,
            item.IsRequired,
            item.Severity)).ToArray();

        var template = new InspectionTemplate(
            TemplateId,
            "Electrical and Safety Inspection",
            1,
            true,
            templateItems);

        if (!await dbContext.Projects.AnyAsync(item => item.Id == ProjectId, cancellationToken))
        {
            dbContext.Projects.Add(project);
        }

        var locationIds = locations.Select(location => location.Id).ToArray();
        var existingLocationIds = await dbContext.ProjectLocations
            .Where(location => locationIds.Contains(location.Id))
            .Select(location => location.Id)
            .ToArrayAsync(cancellationToken);
        dbContext.ProjectLocations.AddRange(
            locations.Where(location => !existingLocationIds.Contains(location.Id)));

        if (!await dbContext.InspectionTemplates.AnyAsync(item => item.Id == TemplateId, cancellationToken))
        {
            dbContext.InspectionTemplates.Add(template);
        }

        await dbContext.SaveChangesAsync(cancellationToken);

        var inspections = CreateDemoInspections(now, templateItems);
        var inspectionIds = inspections.Select(inspection => inspection.Id).ToArray();
        var inspectionNumbers = inspections.Select(inspection => inspection.Number).ToArray();
        var existingInspections = await dbContext.Inspections
            .Where(inspection =>
                inspectionIds.Contains(inspection.Id) ||
                inspectionNumbers.Contains(inspection.Number))
            .Select(inspection => new { inspection.Id, inspection.Number })
            .ToArrayAsync(cancellationToken);
        dbContext.Inspections.AddRange(
            inspections.Where(inspection => existingInspections.All(existing =>
                existing.Id != inspection.Id &&
                existing.Number != inspection.Number)));

        await dbContext.SaveChangesAsync(cancellationToken);
        await SynchronizeInspectionNumberCountersAsync(now.Year, cancellationToken);
    }

    private async Task SynchronizeInspectionNumberCountersAsync(
        int currentYear,
        CancellationToken cancellationToken)
    {
        var definition = NumberSeriesDefinitions.Inspections;
        var inspectionNumbers = await dbContext.Inspections
            .AsNoTracking()
            .Select(inspection => inspection.Number)
            .ToArrayAsync(cancellationToken);

        var lastIssuedByYear = new Dictionary<int, long>();
        foreach (var number in inspectionNumbers)
        {
            if (!TryParseNumber(number, definition.Prefix, out var year, out var serialNumber))
            {
                continue;
            }

            if (!lastIssuedByYear.TryGetValue(year, out var currentValue) || serialNumber > currentValue)
            {
                lastIssuedByYear[year] = serialNumber;
            }
        }

        lastIssuedByYear.TryAdd(currentYear, 0);

        var years = lastIssuedByYear.Keys.ToArray();
        var existingCounters = await dbContext.NumberSeriesCounters
            .Where(counter => counter.SeriesName == definition.Name && years.Contains(counter.Year))
            .ToDictionaryAsync(counter => counter.Year, cancellationToken);

        foreach (var (year, lastIssuedValue) in lastIssuedByYear)
        {
            if (existingCounters.TryGetValue(year, out var counter))
            {
                counter.AdvanceTo(lastIssuedValue);
            }
            else
            {
                dbContext.NumberSeriesCounters.Add(
                    NumberSeriesCounter.Create(definition.Name, year, lastIssuedValue));
            }
        }

        await dbContext.SaveChangesAsync(cancellationToken);
    }

    private static bool TryParseNumber(
        string number,
        string expectedPrefix,
        out int year,
        out long serialNumber)
    {
        year = 0;
        serialNumber = 0;
        var parts = number.Split('-', StringSplitOptions.TrimEntries);

        return parts.Length == 3 &&
            string.Equals(parts[0], expectedPrefix, StringComparison.OrdinalIgnoreCase) &&
            int.TryParse(parts[1], out year) &&
            long.TryParse(parts[2], out serialNumber) &&
            year > 0 &&
            serialNumber >= 0;
    }

    private static List<Inspection> CreateDemoInspections(
        DateTimeOffset now,
        IReadOnlyCollection<InspectionTemplateItem> templateItems)
    {
        var inspections = new List<Inspection>
        {
            CreateInspection(
                AssignedInspectionId,
                "INS-2026-001",
                InspectionStatus.Assigned,
                now.AddDays(5),
                null,
                null,
                null,
                templateItems),
            CreateInspection(
                CompletedInspectionId,
                "INS-2026-000",
                InspectionStatus.Completed,
                now.AddDays(-10),
                now.AddDays(-14),
                now.AddDays(-13),
                now.AddDays(-12),
                templateItems),
        };

        var statuses = Enum.GetValues<InspectionStatus>();
        for (var number = 2; number < 30; number++)
        {
            var status = statuses[(number - 2) % statuses.Length];
            var dueAtUtc = now.AddDays(((number * 3) % 31) - 15);
            var (startedAtUtc, submittedAtUtc, completedAtUtc) = CreateLifecycleDates(number, status, now);

            inspections.Add(CreateInspection(
                CreateStableInspectionId(number + 1),
                $"INS-2026-{number:000}",
                status,
                dueAtUtc,
                startedAtUtc,
                submittedAtUtc,
                completedAtUtc,
                templateItems));
        }

        return inspections;
    }

    private static (DateTimeOffset? StartedAtUtc, DateTimeOffset? SubmittedAtUtc, DateTimeOffset? CompletedAtUtc)
        CreateLifecycleDates(int number, InspectionStatus status, DateTimeOffset now)
    {
        var activityAtUtc = now.AddDays(-((number % 8) + 1));

        return status switch
        {
            InspectionStatus.InProgress => (activityAtUtc, null, null),
            InspectionStatus.Submitted => (activityAtUtc.AddDays(-2), activityAtUtc, null),
            InspectionStatus.CorrectiveActionsOpen => (activityAtUtc.AddDays(-3), activityAtUtc, null),
            InspectionStatus.Completed => (
                activityAtUtc.AddDays(-4),
                activityAtUtc.AddDays(-2),
                activityAtUtc),
            _ => (null, null, null),
        };
    }

    private static Inspection CreateInspection(
        Guid id,
        string number,
        InspectionStatus status,
        DateTimeOffset dueAtUtc,
        DateTimeOffset? startedAtUtc,
        DateTimeOffset? submittedAtUtc,
        DateTimeOffset? completedAtUtc,
        IReadOnlyCollection<InspectionTemplateItem> templateItems)
    {
        var observations = templateItems.Select(item =>
        {
            var outcome = CreateDemoOutcome(status, item.DisplayOrder);
            return new InspectionObservation(
                Guid.CreateVersion7(),
                id,
                item.Id,
                item.SectionName,
                item.Question,
                item.DisplayOrder,
                item.IsRequired,
                outcome,
                item.DefaultSeverity,
                CreateDemoNotes(outcome),
                outcome is null ? null : submittedAtUtc ?? startedAtUtc,
                []);
        }).ToArray();

        return new Inspection(
            id,
            number,
            ProjectId,
            CorridorId,
            TemplateId,
            1,
            InspectorUserId,
            status,
            dueAtUtc,
            startedAtUtc,
            submittedAtUtc,
            completedAtUtc,
            status == InspectionStatus.Cancelled ? startedAtUtc ?? dueAtUtc.AddDays(-1) : null,
            status == InspectionStatus.Cancelled ? "Cancelled demo inspection." : null,
            status == InspectionStatus.InProgress ? startedAtUtc : null,
            observations);
    }

    private static ObservationOutcome? CreateDemoOutcome(InspectionStatus status, int displayOrder) =>
        status switch
        {
            InspectionStatus.InProgress when displayOrder <= 4 =>
                displayOrder == 3 ? ObservationOutcome.Fail : ObservationOutcome.Pass,
            InspectionStatus.Submitted =>
                displayOrder % 4 == 0 ? ObservationOutcome.Fail : ObservationOutcome.Pass,
            InspectionStatus.CorrectiveActionsOpen =>
                displayOrder % 3 == 0 ? ObservationOutcome.Fail : ObservationOutcome.Pass,
            InspectionStatus.Completed when displayOrder == 10 => ObservationOutcome.NotApplicable,
            InspectionStatus.Completed => ObservationOutcome.Pass,
            _ => null,
        };

    private static string? CreateDemoNotes(ObservationOutcome? outcome) => outcome switch
    {
        ObservationOutcome.Pass => "Verified during the demo inspection.",
        ObservationOutcome.Fail => "Demo finding: corrective work and follow-up verification are required.",
        ObservationOutcome.NotApplicable => "Not applicable at this demo location.",
        _ => null,
    };

    private static Guid CreateStableTemplateItemId(int itemNumber) =>
        Guid.Parse($"019c9a10-0021-7000-8000-{itemNumber:000000000000}");

    private static Guid CreateStableInspectionId(int inspectionNumber) =>
        Guid.Parse($"019c9a10-0030-7000-8000-{inspectionNumber:000000000000}");

    private static void EnsureSucceeded(IdentityResult result, string operation)
    {
        if (result.Succeeded)
        {
            return;
        }

        var errors = string.Join(", ", result.Errors.Select(error => error.Description));
        throw new InvalidOperationException($"Unable to {operation}: {errors}");
    }

    [LoggerMessage(Level = LogLevel.Information, Message = "SiteInspect demo data is ready")]
    private static partial void LogDemoDataReady(ILogger logger);

}

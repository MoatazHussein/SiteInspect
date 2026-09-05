using Microsoft.EntityFrameworkCore;
using SiteInspect.Domain.Inspections;
using SiteInspect.Domain.Inspections.InspectionAggregate;
using SiteInspect.Domain.Inspections.InspectionTemplateAggregate;

namespace SiteInspect.Infrastructure.Persistence.Initialization.Steps;

internal sealed class InspectionsSeedStep(ApplicationDbContext dbContext, TimeProvider timeProvider) : ISeedStep
{
    public string Name => "Demo inspections";
    public int Order => SeedStepOrder.Inspections;

    public async Task SeedAsync(CancellationToken cancellationToken)
    {
        var now = timeProvider.GetUtcNow();
        var templateItems = DemoSeedData.CreateTemplateItems();
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
    }

    private static List<Inspection> CreateDemoInspections(
        DateTimeOffset now,
        IReadOnlyCollection<InspectionTemplateItem> templateItems)
    {
        var inspections = new List<Inspection>
        {
            CreateInspection(
                DemoSeedData.AssignedInspectionId,
                "INS-2026-001",
                InspectionStatus.Assigned,
                now.AddDays(5),
                null,
                null,
                null,
                templateItems),
            CreateInspection(
                DemoSeedData.CompletedInspectionId,
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
            DemoSeedData.ProjectId,
            DemoSeedData.CorridorId,
            DemoSeedData.TemplateId,
            1,
            DemoSeedData.InspectorUserId,
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

    private static Guid CreateStableInspectionId(int inspectionNumber) =>
        Guid.Parse($"019c9a10-0030-7000-8000-{inspectionNumber:000000000000}");
}

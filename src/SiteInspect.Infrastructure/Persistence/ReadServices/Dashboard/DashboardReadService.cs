using Microsoft.EntityFrameworkCore;
using SiteInspect.Application.Features.Dashboard.Common;
using SiteInspect.Application.Features.Dashboard.Queries.GetDashboardSummary;
using SiteInspect.Domain.CorrectiveActions.CorrectiveActionAggregate;
using SiteInspect.Domain.Inspections.InspectionAggregate;

namespace SiteInspect.Infrastructure.Persistence.ReadServices.Dashboard;

internal sealed class DashboardReadService(ApplicationDbContext dbContext, TimeProvider timeProvider)
    : IDashboardReadService
{
    public async Task<DashboardSummaryResponse> GetSummaryAsync(GetDashboardSummaryQuery query, CancellationToken cancellationToken)
    {
        var now = timeProvider.GetUtcNow();
        var scopedInspections = dbContext.Inspections.AsNoTracking().AsQueryable();
        if (query.ProjectId is Guid projectId)
        {
            scopedInspections = scopedInspections.Where(item => item.ProjectId == projectId);
        }
        if (query.Status is InspectionStatus status)
        {
            scopedInspections = scopedInspections.Where(item => item.Status == status);
        }

        var inspectionQuery = scopedInspections;
        var actionQuery = dbContext.CorrectiveActions.AsNoTracking()
            .Where(action => scopedInspections.Select(item => item.Id).Contains(action.InspectionId));
        if (query.DueFromUtc is DateTimeOffset from)
        {
            inspectionQuery = inspectionQuery.Where(item => item.DueAtUtc >= from);
            actionQuery = actionQuery.Where(item => item.DueAtUtc >= from);
        }
        if (query.DueBeforeUtc is DateTimeOffset before)
        {
            inspectionQuery = inspectionQuery.Where(item => item.DueAtUtc < before);
            actionQuery = actionQuery.Where(item => item.DueAtUtc < before);
        }

        var inspections = await inspectionQuery
            .GroupBy(item => 1)
            .Select(group => new
            {
                Active = group.Count(item => item.Status == InspectionStatus.Assigned ||
                    item.Status == InspectionStatus.InProgress || item.Status == InspectionStatus.Submitted ||
                    item.Status == InspectionStatus.CorrectiveActionsOpen),
                Overdue = group.Count(item => item.DueAtUtc < now &&
                    (item.Status == InspectionStatus.Assigned || item.Status == InspectionStatus.InProgress)),
                Completed = group.Count(item => item.Status == InspectionStatus.Completed),
            })
            .SingleOrDefaultAsync(cancellationToken);

        var actions = await actionQuery
            .GroupBy(item => 1)
            .Select(group => new
            {
                Outstanding = group.Count(item => item.Status != CorrectiveActionStatus.Closed),
                AwaitingReview = group.Count(item => item.Status == CorrectiveActionStatus.ReadyForReview),
                Overdue = group.Count(item => item.Status != CorrectiveActionStatus.Closed && item.DueAtUtc < now),
            })
            .SingleOrDefaultAsync(cancellationToken);

        var overdueItems = await (
            from item in inspectionQuery
            join project in dbContext.Projects on item.ProjectId equals project.Id
            join inspector in dbContext.Users on item.AssignedInspectorId equals inspector.Id
            where item.DueAtUtc < now &&
                (item.Status == InspectionStatus.Assigned || item.Status == InspectionStatus.InProgress)
            orderby item.DueAtUtc, item.Id
            select new DashboardInspectionItem(item.Id, item.Number, project.Name,
                inspector.DisplayName, item.Status, item.DueAtUtc))
            .Take(10).ToArrayAsync(cancellationToken);

        var outstandingItems = await (
            from action in actionQuery
            join inspection in dbContext.Inspections on action.InspectionId equals inspection.Id
            join project in dbContext.Projects on inspection.ProjectId equals project.Id
            join contractor in dbContext.Users on action.AssignedContractorId equals contractor.Id
            where action.Status != CorrectiveActionStatus.Closed
            orderby action.DueAtUtc, action.Id
            select new DashboardActionItem(action.Id, inspection.Id, inspection.Number,
                project.Name, contractor.DisplayName, action.Description, action.Status, action.DueAtUtc))
            .Take(10).ToArrayAsync(cancellationToken);

        return new DashboardSummaryResponse(
            inspections?.Active ?? 0, inspections?.Overdue ?? 0, inspections?.Completed ?? 0,
            actions?.Outstanding ?? 0, actions?.AwaitingReview ?? 0, actions?.Overdue ?? 0, now,
            overdueItems, outstandingItems);
    }
}

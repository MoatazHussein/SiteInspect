using Microsoft.EntityFrameworkCore;
using SiteInspect.Application.Common.Authorization;
using SiteInspect.Application.Common.Results;
using SiteInspect.Application.Features.CorrectiveActions.Common;
using SiteInspect.Application.Features.Inspections;

namespace SiteInspect.Infrastructure.Persistence.ReadServices.CorrectiveActions;

internal sealed class CorrectiveActionReadService(ApplicationDbContext dbContext) : ICorrectiveActionReadService
{
    public async Task<Result<IReadOnlyCollection<CorrectiveActionResponse>>> ListAsync(
        Guid inspectionId, CancellationToken cancellationToken)
    {
        if (!await dbContext.Inspections.AnyAsync(item => item.Id == inspectionId, cancellationToken))
        {
            return Result<IReadOnlyCollection<CorrectiveActionResponse>>.Failure(InspectionErrors.NotFound(inspectionId));
        }

        var rows = await (
            from action in dbContext.CorrectiveActions.AsNoTracking()
            join contractor in dbContext.Users.AsNoTracking() on action.AssignedContractorId equals contractor.Id
            where action.InspectionId == inspectionId
            orderby action.DueAtUtc, action.Id
            select new { Action = action, ContractorName = contractor.DisplayName })
            .ToArrayAsync(cancellationToken);

        return Result<IReadOnlyCollection<CorrectiveActionResponse>>.Success(rows.Select(row =>
            new CorrectiveActionResponse(row.Action.Id, row.Action.InspectionId, row.Action.ObservationId,
                row.Action.AssignedContractorId, row.ContractorName, row.Action.Description,
                row.Action.DueAtUtc, row.Action.Status, row.Action.ResolutionNotes,
                row.Action.RejectionReason, row.Action.SubmittedAtUtc, row.Action.RejectedAtUtc,
                row.Action.ClosedAtUtc, row.Action.CreatedAtUtc,
                Convert.ToBase64String(row.Action.RowVersion))).ToArray());
    }

    public async Task<Result<IReadOnlyCollection<ContractorCorrectiveActionResponse>>> ListAssignedAsync(
        Guid contractorId,
        CancellationToken cancellationToken)
    {
        var rows = await (
            from action in dbContext.CorrectiveActions.AsNoTracking()
            join inspection in dbContext.Inspections.AsNoTracking() on action.InspectionId equals inspection.Id
            join observation in dbContext.InspectionObservations.AsNoTracking()
                on action.ObservationId equals observation.Id
            where action.AssignedContractorId == contractorId
            orderby action.Status, action.DueAtUtc, action.Id
            select new
            {
                Action = action,
                InspectionNumber = inspection.Number,
                ObservationDisplayOrder = observation.DisplayOrderSnapshot,
                ObservationQuestion = observation.QuestionSnapshot,
            })
            .ToArrayAsync(cancellationToken);

        return Result<IReadOnlyCollection<ContractorCorrectiveActionResponse>>.Success(rows.Select(row =>
            new ContractorCorrectiveActionResponse(
                row.Action.Id,
                row.Action.InspectionId,
                row.InspectionNumber,
                row.Action.ObservationId,
                row.ObservationDisplayOrder,
                row.ObservationQuestion,
                row.Action.Description,
                row.Action.DueAtUtc,
                row.Action.Status,
                row.Action.ResolutionNotes,
                row.Action.RejectionReason,
                row.Action.SubmittedAtUtc,
                row.Action.RejectedAtUtc,
                row.Action.ClosedAtUtc,
                row.Action.CreatedAtUtc,
                Convert.ToBase64String(row.Action.RowVersion))).ToArray());
    }

    public async Task<IReadOnlyCollection<ContractorOption>> GetContractorsAsync(CancellationToken cancellationToken) =>
        await (from user in dbContext.Users.AsNoTracking()
               join userRole in dbContext.UserRoles.AsNoTracking() on user.Id equals userRole.UserId
               join role in dbContext.Roles.AsNoTracking() on userRole.RoleId equals role.Id
               where role.Name == RoleNames.Contractor
               orderby user.DisplayName, user.Id
               select new ContractorOption(user.Id, user.DisplayName))
            .ToArrayAsync(cancellationToken);
}


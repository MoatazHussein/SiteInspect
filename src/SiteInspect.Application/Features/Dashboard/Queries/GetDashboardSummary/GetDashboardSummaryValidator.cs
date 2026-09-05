using FluentValidation;

namespace SiteInspect.Application.Features.Dashboard.Queries.GetDashboardSummary;

public sealed class GetDashboardSummaryValidator : AbstractValidator<GetDashboardSummaryQuery>
{
    public GetDashboardSummaryValidator()
    {
        RuleFor(query => query.Status).IsInEnum().When(query => query.Status.HasValue);
        RuleFor(query => query.ProjectId).NotEqual(Guid.Empty).When(query => query.ProjectId.HasValue);
        RuleFor(query => query.DueBeforeUtc)
            .Must((query, end) => !end.HasValue || !query.DueFromUtc.HasValue || end > query.DueFromUtc)
            .WithMessage("The end of the due-date range must be after its start.");
    }
}

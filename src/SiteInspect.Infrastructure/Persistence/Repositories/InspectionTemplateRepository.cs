using Microsoft.EntityFrameworkCore;
using SiteInspect.Application.Features.Inspections.Common;
using SiteInspect.Domain.Inspections.InspectionTemplateAggregate;

namespace SiteInspect.Infrastructure.Persistence.Repositories;

internal sealed class InspectionTemplateRepository(ApplicationDbContext dbContext)
    : IInspectionTemplateRepository
{
    public Task<InspectionTemplate?> GetActiveWithItemsAsync(
        Guid templateId,
        CancellationToken cancellationToken = default) =>
        dbContext.InspectionTemplates
            .Include(template => template.Items)
            .SingleOrDefaultAsync(
                template => template.Id == templateId && template.IsActive,
                cancellationToken);
}

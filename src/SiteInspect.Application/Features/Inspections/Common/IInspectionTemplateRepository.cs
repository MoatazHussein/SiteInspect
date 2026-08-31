using SiteInspect.Domain.Inspections.InspectionTemplateAggregate;

namespace SiteInspect.Application.Features.Inspections.Common;

public interface IInspectionTemplateRepository
{
    Task<InspectionTemplate?> GetActiveWithItemsAsync(
        Guid templateId,
        CancellationToken cancellationToken = default);
}

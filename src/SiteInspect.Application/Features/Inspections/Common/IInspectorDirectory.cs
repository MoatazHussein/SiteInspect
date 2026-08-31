namespace SiteInspect.Application.Features.Inspections.Common;

public interface IInspectorDirectory
{
    Task<bool> ExistsAsync(Guid inspectorId, CancellationToken cancellationToken = default);
}

namespace SiteInspect.Application.Features.CorrectiveActions.Common;

public interface IContractorDirectory
{
    Task<bool> ExistsAsync(Guid contractorId, CancellationToken cancellationToken = default);
}


using SiteInspect.Application.Common.Results;

namespace SiteInspect.Application.Features.CorrectiveActions.Common;

public interface ICorrectiveActionReadService
{
    Task<Result<IReadOnlyCollection<CorrectiveActionResponse>>> ListAsync(Guid inspectionId, CancellationToken cancellationToken);
    Task<Result<IReadOnlyCollection<ContractorCorrectiveActionResponse>>> ListAssignedAsync(
        Guid contractorId,
        CancellationToken cancellationToken);
    Task<IReadOnlyCollection<ContractorOption>> GetContractorsAsync(CancellationToken cancellationToken);
}


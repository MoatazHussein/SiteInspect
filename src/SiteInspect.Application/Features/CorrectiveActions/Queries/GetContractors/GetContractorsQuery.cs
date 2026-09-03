using SiteInspect.Application.Common.Messaging;
using SiteInspect.Application.Features.CorrectiveActions.Common;

namespace SiteInspect.Application.Features.CorrectiveActions.Queries.GetContractors;

public sealed record GetContractorsQuery : IQuery<IReadOnlyCollection<ContractorOption>>;


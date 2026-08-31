using MediatR;
using SiteInspect.Application.Common.Results;

namespace SiteInspect.Application.Common.Messaging;

public interface IQuery<TResponse> : IRequest<Result<TResponse>>;

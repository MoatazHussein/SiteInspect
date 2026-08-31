using MediatR;
using SiteInspect.Application.Common.Results;

namespace SiteInspect.Application.Common.Messaging;

public interface ICommand : IRequest<Result>;

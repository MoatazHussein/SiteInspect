using SiteInspect.Application.Common.Messaging;

namespace SiteInspect.Application.Features.Authentication.Commands.RefreshSession;

public sealed record RefreshSessionCommand(string RefreshToken) : ICommand<RefreshSessionResponse>;

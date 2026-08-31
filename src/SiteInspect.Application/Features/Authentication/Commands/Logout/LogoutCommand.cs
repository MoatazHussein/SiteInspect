using SiteInspect.Application.Common.Messaging;

namespace SiteInspect.Application.Features.Authentication.Commands.Logout;

public sealed record LogoutCommand(string? RefreshToken) : ICommand;

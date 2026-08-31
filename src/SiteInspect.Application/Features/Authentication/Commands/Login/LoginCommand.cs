using SiteInspect.Application.Common.Messaging;

namespace SiteInspect.Application.Features.Authentication.Commands.Login;

public sealed record LoginCommand(string Email, string Password) : ICommand<LoginResponse>;

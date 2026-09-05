using MediatR;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SiteInspect.Api.Contracts;
using SiteInspect.Api.ErrorHandling;
using SiteInspect.Application.Common.Abstractions.Identity;
using SiteInspect.Application.Common.Errors;
using SiteInspect.Application.Common.Results;
using SiteInspect.Application.Features.Authentication.Commands.Login;
using SiteInspect.Application.Features.Authentication.Commands.Logout;
using SiteInspect.Application.Features.Authentication.Commands.RefreshSession;
using SiteInspect.Application.Features.Authentication.Queries.GetCurrentUser;

namespace SiteInspect.Api.Controllers;

[ApiController]
[Route("api/auth")]
public sealed class AuthenticationController(
    ISender sender,
    ICurrentUser currentUser) : ControllerBase
{
    private const string RefreshCookieName = "siteinspect.refresh";

    [AllowAnonymous]
    [HttpPost("login")]
    [EnableRateLimiting("login")]
    public async Task<IActionResult> Login(
        [FromBody] LoginCommand command,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(command, cancellationToken);
        if (result.IsFailure)
        {
            return Result<AuthenticationSessionResponse>
                .Failure(result.Errors)
                .ToActionResult(HttpContext);
        }

        var response = result.Value;
        SetRefreshCookie(response.RefreshToken, response.RefreshTokenExpiresAtUtc);

        return Result<AuthenticationSessionResponse>.Success(new AuthenticationSessionResponse(
            response.AccessToken,
            response.AccessTokenExpiresAtUtc,
            response.User)).ToActionResult(HttpContext);
    }

    [AllowAnonymous]
    [HttpPost("refresh")]
    public async Task<IActionResult> Refresh(CancellationToken cancellationToken)
    {
        Request.Cookies.TryGetValue(RefreshCookieName, out var refreshToken);
        var result = await sender.Send(
            new RefreshSessionCommand(refreshToken ?? string.Empty),
            cancellationToken);

        if (result.IsFailure)
        {
            return Result<AuthenticationSessionResponse>
                .Failure(result.Errors)
                .ToActionResult(HttpContext);
        }

        var response = result.Value;
        SetRefreshCookie(response.RefreshToken, response.RefreshTokenExpiresAtUtc);

        return Result<AuthenticationSessionResponse>.Success(new AuthenticationSessionResponse(
            response.AccessToken,
            response.AccessTokenExpiresAtUtc,
            response.User)).ToActionResult(HttpContext);
    }

    [AllowAnonymous]
    [HttpPost("logout")]
    public async Task<IActionResult> Logout(CancellationToken cancellationToken)
    {
        Request.Cookies.TryGetValue(RefreshCookieName, out var refreshToken);
        var result = await sender.Send(new LogoutCommand(refreshToken), cancellationToken);
        DeleteRefreshCookie();

        return result.ToActionResult(HttpContext);
    }

    [Authorize]
    [HttpGet("me")]
    public async Task<IActionResult> GetCurrentUser(CancellationToken cancellationToken)
    {
        if (currentUser.UserId is not Guid userId)
        {
            return Result<GetCurrentUserResponse>
                .Failure(AuthenticationErrors.AuthenticationRequired)
                .ToActionResult(HttpContext);
        }

        var result = await sender.Send(
            new GetCurrentUserQuery(userId),
            cancellationToken);

        return result.ToActionResult(HttpContext);
    }

    private void SetRefreshCookie(string refreshToken, DateTimeOffset expiresAtUtc) =>
        Response.Cookies.Append(
            RefreshCookieName,
            refreshToken,
            CreateCookieOptions(expiresAtUtc));

    private void DeleteRefreshCookie() =>
        Response.Cookies.Delete(
            RefreshCookieName,
            CreateCookieOptions(DateTimeOffset.UnixEpoch));

    private CookieOptions CreateCookieOptions(DateTimeOffset expiresAtUtc) => new()
    {
        HttpOnly = true,
        Secure = Request.IsHttps,
        SameSite = SameSiteMode.Strict,
        Path = "/api/auth",
        Expires = expiresAtUtc,
        IsEssential = true,
    };
}

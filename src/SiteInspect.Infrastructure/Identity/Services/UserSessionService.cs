using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using SiteInspect.Application.Common.Errors;
using SiteInspect.Application.Common.Exceptions;
using SiteInspect.Application.Common.Results;
using SiteInspect.Application.Features.Authentication.Common;
using SiteInspect.Infrastructure.Identity.Entities;
using SiteInspect.Infrastructure.Identity.Options;
using SiteInspect.Infrastructure.Persistence;

namespace SiteInspect.Infrastructure.Identity.Services;

internal sealed class UserSessionService(
    UserManager<ApplicationUser> userManager,
    ApplicationDbContext dbContext,
    TimeProvider timeProvider,
    IOptions<JwtOptions> jwtOptions) : IUserSessionService
{
    private readonly JwtOptions options = jwtOptions.Value;

    public async Task<Result<AuthenticatedSession>> LoginAsync(
        string email,
        string password,
        CancellationToken cancellationToken = default)
    {
        var user = await userManager.FindByEmailAsync(email.Trim());

        if (user is null || user.LockoutEnd > timeProvider.GetUtcNow())
        {
            return Result<AuthenticatedSession>.Failure(AuthenticationErrors.InvalidCredentials);
        }

        if (!await userManager.CheckPasswordAsync(user, password))
        {
            await userManager.AccessFailedAsync(user);
            return Result<AuthenticatedSession>.Failure(AuthenticationErrors.InvalidCredentials);
        }

        await userManager.ResetAccessFailedCountAsync(user);
        return await CreateSessionAsync(user, cancellationToken);
    }

    public async Task<Result<AuthenticatedSession>> RefreshAsync(
        string refreshToken,
        CancellationToken cancellationToken = default)
    {
        var now = timeProvider.GetUtcNow();
        var tokenHash = HashToken(refreshToken);
        var storedToken = await dbContext.RefreshTokens
            .SingleOrDefaultAsync(token => token.TokenHash == tokenHash, cancellationToken);

        if (storedToken is null)
        {
            return Result<AuthenticatedSession>.Failure(AuthenticationErrors.InvalidRefreshToken);
        }

        if (!storedToken.IsActive(now))
        {
            if (storedToken.RevokedAtUtc is not null)
            {
                await RevokeAllActiveTokensAsync(storedToken.UserId, now, cancellationToken);
            }

            return Result<AuthenticatedSession>.Failure(AuthenticationErrors.InvalidRefreshToken);
        }

        var user = await userManager.FindByIdAsync(storedToken.UserId.ToString());
        if (user is null || !string.Equals(user.SecurityStamp, storedToken.SecurityStamp, StringComparison.Ordinal))
        {
            await RevokeAllActiveTokensAsync(storedToken.UserId, now, cancellationToken);
            return Result<AuthenticatedSession>.Failure(AuthenticationErrors.InvalidRefreshToken);
        }

        var replacement = CreateRefreshToken(user, now);
        storedToken.Revoke(now, replacement.Entity.Id);
        dbContext.RefreshTokens.Add(replacement.Entity);

        try
        {
            await dbContext.SaveChangesAsync(cancellationToken);
        }
        catch (PersistenceConcurrencyException)
        {
            return Result<AuthenticatedSession>.Failure(AuthenticationErrors.InvalidRefreshToken);
        }

        return Result<AuthenticatedSession>.Success(
            await CreateSessionResponseAsync(user, replacement.RawToken, replacement.Entity.ExpiresAtUtc));
    }

    public async Task RevokeAsync(
        string? refreshToken,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(refreshToken))
        {
            return;
        }

        var tokenHash = HashToken(refreshToken);
        var storedToken = await dbContext.RefreshTokens
            .SingleOrDefaultAsync(token => token.TokenHash == tokenHash, cancellationToken);

        if (storedToken is null || storedToken.RevokedAtUtc is not null)
        {
            return;
        }

        storedToken.Revoke(timeProvider.GetUtcNow());
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task<Result<AuthenticatedUser>> GetCurrentUserAsync(
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        var user = await userManager.FindByIdAsync(userId.ToString());
        if (user is null)
        {
            return Result<AuthenticatedUser>.Failure(AuthenticationErrors.UserNotFound);
        }

        return Result<AuthenticatedUser>.Success(await CreateCurrentUserAsync(user));
    }

    private async Task<Result<AuthenticatedSession>> CreateSessionAsync(
        ApplicationUser user,
        CancellationToken cancellationToken)
    {
        var now = timeProvider.GetUtcNow();
        var refreshToken = CreateRefreshToken(user, now);
        dbContext.RefreshTokens.Add(refreshToken.Entity);
        await dbContext.SaveChangesAsync(cancellationToken);

        return Result<AuthenticatedSession>.Success(
            await CreateSessionResponseAsync(user, refreshToken.RawToken, refreshToken.Entity.ExpiresAtUtc));
    }

    private async Task<AuthenticatedSession> CreateSessionResponseAsync(
        ApplicationUser user,
        string rawRefreshToken,
        DateTimeOffset refreshTokenExpiresAtUtc)
    {
        var currentUser = await CreateCurrentUserAsync(user);
        var accessTokenExpiresAtUtc = timeProvider.GetUtcNow().AddMinutes(options.AccessTokenMinutes);
        var accessToken = CreateAccessToken(currentUser, accessTokenExpiresAtUtc);

        return new AuthenticatedSession(
            accessToken,
            accessTokenExpiresAtUtc,
            rawRefreshToken,
            refreshTokenExpiresAtUtc,
            currentUser);
    }

    private async Task<AuthenticatedUser> CreateCurrentUserAsync(ApplicationUser user)
    {
        var roles = await userManager.GetRolesAsync(user);

        return new AuthenticatedUser(
            user.Id,
            user.Email ?? string.Empty,
            user.DisplayName,
            roles.Order(StringComparer.Ordinal).ToArray());
    }

    private string CreateAccessToken(AuthenticatedUser user, DateTimeOffset expiresAtUtc)
    {
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new(ClaimTypes.Email, user.Email),
            new(ClaimTypes.Name, user.DisplayName),
            new(JwtRegisteredClaimNames.Jti, Guid.CreateVersion7().ToString()),
        };

        claims.AddRange(user.Roles.Select(role => new Claim(ClaimTypes.Role, role)));

        var credentials = new SigningCredentials(
            new SymmetricSecurityKey(Encoding.UTF8.GetBytes(options.SigningKey)),
            SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            options.Issuer,
            options.Audience,
            claims,
            notBefore: timeProvider.GetUtcNow().UtcDateTime,
            expires: expiresAtUtc.UtcDateTime,
            signingCredentials: credentials);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    private (RefreshToken Entity, string RawToken) CreateRefreshToken(
        ApplicationUser user,
        DateTimeOffset now)
    {
        var rawToken = GenerateToken();
        var entity = new RefreshToken(
            Guid.CreateVersion7(),
            user.Id,
            HashToken(rawToken),
            user.SecurityStamp ?? string.Empty,
            now,
            now.AddDays(options.RefreshTokenDays));

        return (entity, rawToken);
    }

    private async Task RevokeAllActiveTokensAsync(
        Guid userId,
        DateTimeOffset revokedAtUtc,
        CancellationToken cancellationToken)
    {
        var activeTokens = await dbContext.RefreshTokens
            .Where(token => token.UserId == userId && token.RevokedAtUtc == null)
            .ToListAsync(cancellationToken);

        foreach (var token in activeTokens)
        {
            token.Revoke(revokedAtUtc);
        }

        if (activeTokens.Count > 0)
        {
            await dbContext.SaveChangesAsync(cancellationToken);
        }
    }

    private static string GenerateToken()
    {
        var token = Convert.ToBase64String(RandomNumberGenerator.GetBytes(64));
        return token.TrimEnd('=').Replace('+', '-').Replace('/', '_');
    }

    private static string HashToken(string token) =>
        Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(token)));
}

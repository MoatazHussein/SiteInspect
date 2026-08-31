using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using SiteInspect.Api.ErrorHandling;
using SiteInspect.Application.Common.Errors;
using SiteInspect.Infrastructure.Identity.Options;

namespace SiteInspect.Api.Authentication;

internal static class AuthenticationExtensions
{
    public static IServiceCollection AddApiAuthentication(
        this IServiceCollection services,
        IConfiguration configuration,
        IHostEnvironment environment)
    {
        var issuer = configuration["Jwt:Issuer"] ?? "SiteInspect";
        var audience = configuration["Jwt:Audience"] ?? "SiteInspect.Web";
        var signingKey = configuration["Jwt:SigningKey"];

        if (string.IsNullOrWhiteSpace(signingKey))
        {
            if (environment.IsProduction())
            {
                throw new InvalidOperationException("Jwt:SigningKey must be supplied outside source control.");
            }

            signingKey = Convert.ToBase64String(RandomNumberGenerator.GetBytes(64));
        }

        var jwtOptions = new JwtOptions
        {
            Issuer = issuer,
            Audience = audience,
            SigningKey = signingKey,
            AccessTokenMinutes = configuration.GetValue("Jwt:AccessTokenMinutes", 30),
            RefreshTokenDays = configuration.GetValue("Jwt:RefreshTokenDays", 7),
        };

        if (jwtOptions.AccessTokenMinutes <= 0 || jwtOptions.RefreshTokenDays <= 0)
        {
            throw new InvalidOperationException("JWT token lifetimes must be greater than zero.");
        }

        services.AddSingleton<IOptions<JwtOptions>>(Options.Create(jwtOptions));

        services
            .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidIssuer = jwtOptions.Issuer,
                    ValidateAudience = true,
                    ValidAudience = jwtOptions.Audience,
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtOptions.SigningKey)),
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.FromMinutes(1),
                    NameClaimType = System.Security.Claims.ClaimTypes.Name,
                    RoleClaimType = System.Security.Claims.ClaimTypes.Role,
                };

                options.Events = new JwtBearerEvents
                {
                    OnChallenge = context =>
                    {
                        context.HandleResponse();

                        return ApiResponseWriter.WriteFailureAsync(
                            context.HttpContext,
                            StatusCodes.Status401Unauthorized,
                            [AuthenticationErrors.AuthenticationRequired],
                            context.HttpContext.RequestAborted);
                    },
                    OnForbidden = context => ApiResponseWriter.WriteFailureAsync(
                        context.HttpContext,
                        StatusCodes.Status403Forbidden,
                        [AuthorizationErrors.Forbidden],
                        context.HttpContext.RequestAborted),
                };
            });

        return services;
    }
}

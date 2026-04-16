using Bixa.Backend.Base;
using Bixa.Backend.Controllers.Services;
using Bixa.Backend.Models.Auth;
using Bixa.Backend.Services.Services.JwtControllers;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.IdentityModel.Tokens;
using System.Text;

namespace Bixa.Backend.Configuration;

/// <summary>
/// Provides extension methods to configure JWT authentication and related services.
/// </summary>
public static class AuthConfigurationExtensions
{
    /// <summary>
    /// Configures JWT authentication, registers JWT services, and sets up custom JWT events.
    /// This method retrieves JWT settings from the application's configuration.
    /// </summary>
    /// <param name="services">The <see cref="IServiceCollection"/> to which services will be added.</param>
    /// <param name="configuration">The <see cref="IConfiguration"/> instance used to retrieve JWT settings.</param>
    /// <exception cref="InvalidOperationException">
    /// Thrown if essential JWT configuration values (Key, Issuer, Audience) are missing or invalid in appsettings.json.
    /// </exception>
    public static void ConfigureJwtAuthenticationAndServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<JwtConfiguration>(configuration.GetSection(JwtConfiguration.SectionName));

        var jwtConfig = configuration.GetSection(JwtConfiguration.SectionName).Get<JwtConfiguration>();

        if (jwtConfig == null || string.IsNullOrEmpty(jwtConfig.Llave) || string.IsNullOrEmpty(jwtConfig.Issuer) || string.IsNullOrEmpty(jwtConfig.Audience))
            throw new InvalidOperationException($"JWT configuration (Key, Issuer, Audience) is missing or invalid in appsettings.json. Section: '{JwtConfiguration.SectionName}'.");

        services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        })
        .AddJwtBearer(options =>
        {
            options.TokenValidationParameters = new TokenValidationParameters()
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                ValidIssuer = jwtConfig.Issuer,
                ValidAudience = jwtConfig.Audience,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtConfig.Llave)),
                ValidAlgorithms = new[] { SecurityAlgorithms.HmacSha256 },
                ClockSkew = TimeSpan.FromMinutes(5),
                RoleClaimType = "http://schemas.microsoft.com/ws/2008/06/identity/claims/role"
            };

            options.Events = new JwtBearerEvents
            {
                OnTokenValidated = context =>
                {
                    var logger = context.HttpContext.RequestServices.GetRequiredService<ILogger<JwtBearerEvents>>();
                    return Task.CompletedTask;
                },
                OnAuthenticationFailed = context =>
                {
                    var logger = context.HttpContext.RequestServices.GetRequiredService<ILogger<JwtBearerEvents>>();
                    context.HttpContext.Response.StatusCode = StatusCodes.Status401Unauthorized;
                    return Task.CompletedTask;
                },
                OnChallenge = context =>
                {
                    var logger = context.HttpContext.RequestServices.GetRequiredService<ILogger<JwtBearerEvents>>();
                    if (context.AuthenticateFailure == null)
                        context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                    else
                        context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                    return Task.CompletedTask;
                },
                OnForbidden = context =>
                {
                    var logger = context.HttpContext.RequestServices.GetRequiredService<ILogger<JwtBearerEvents>>();
                    logger.LogWarning("JWT Forbidden: User does not have sufficient permissions to access '{Path}'.", context.HttpContext.Request.Path);
                    context.Response.StatusCode = StatusCodes.Status403Forbidden;
                    return Task.CompletedTask;
                }
            };
        });

        services.AddSingleton<IAuthorizationMiddlewareResultHandler, CustomAuthorizationMiddlewareResultHandler>();

        services.AddScoped<IManejoJwt, ManejoJwt>();
        services.AddScoped<IJwtAuthService, JwtAuthService>();
    }
}
using Microsoft.AspNetCore.Authorization;
using Bixa.Backend.Models.Utilities;
using Bixa.Backend.Models.Enums;

namespace Bixa.Backend.Configuration;

/// <summary>
/// Utility class for configuring authorization policies in a more modular and reusable way.
/// </summary>
public static class AuthorizationUtils
{
    /// <summary>
    /// Adds standard authorization policies based on UserRolEnum, UserTypeEnum, and SigningRoleEnum capabilities.
    /// </summary>
    /// <param name="services">The service collection to which the policies will be added.</param>
    public static void AddStandardPolicies(this IServiceCollection services)
    {
        // First, register the custom policy provider.
        services.AddSingleton<IAuthorizationPolicyProvider, AdministratorPolicyProvider>();

        services.AddAuthorizationBuilder()
            .AddPolicy(UserRolEnum.Requester.GetDescriptionPolicy(), policy =>
                policy.RequireRole(nameof(UserRolEnum.Requester)))
            .AddPolicy(UserRolEnum.Executive.GetDescriptionPolicy(), policy =>
                policy.RequireRole(nameof(UserRolEnum.Executive)))
            .AddPolicy(UserRolEnum.Executive.GetDescriptionPolicy(), policy =>
                policy.RequireRole(nameof(UserRolEnum.Executive)))
            .AddPolicy("RequesterWithEstablishmentPolicy", policy =>
            {
                policy.RequireRole(nameof(UserRolEnum.Requester));
                policy.RequireClaim("HasEstablishment", "true");
            })
            .AddPolicy("ExecutiveWithEstablishmentPolicy", policy =>
            {
                policy.RequireRole(nameof(UserRolEnum.Executive));
                policy.RequireClaim("HasEstablishment", "true");
            });
    }

    public static string GetDescriptionPolicy(this Enum value) => value.GetDescription() + "Policy";
}
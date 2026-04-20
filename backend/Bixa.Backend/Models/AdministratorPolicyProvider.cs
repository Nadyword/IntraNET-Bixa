using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authorization.Infrastructure;
using Microsoft.Extensions.Options;

public class AdministratorPolicyProvider(IOptions<AuthorizationOptions> options) : DefaultAuthorizationPolicyProvider(options)
{
    private const string AdministratorRole = "Administrator";

    public override async Task<AuthorizationPolicy?> GetPolicyAsync(string policyName)
    {
        // Get the policy from the default provider
        var policy = await base.GetPolicyAsync(policyName);

        // If the policy exists, we check if it already contains the Administrator role.
        if (policy != null)
        {
            // If the policy does not already contain the Administrator role, we create a new policy
            // with the Administrator role added. This is to avoid modifying the policy in place
            // and to ensure that the Administrator role is always present.
            if (!policy.Requirements.Any(r => r is RolesAuthorizationRequirement &&
                                             ((RolesAuthorizationRequirement)r).AllowedRoles.Contains(AdministratorRole)))
            {
                var newPolicyBuilder = new AuthorizationPolicyBuilder(policy);
                newPolicyBuilder.RequireRole(AdministratorRole);

                // This would need to be re-evaluated for more complex policies, but for simple role policies it should work
                // Re-add other roles and requirements
                foreach (var requirement in policy.Requirements)
                {
                    if (requirement is not RolesAuthorizationRequirement)
                        newPolicyBuilder.Requirements.Add(requirement);
                }

                return newPolicyBuilder.Build();
            }
        }
        else
        {
            // If the policy doesn't exist, we create a new one with the Administrator role.
            // This ensures that any typo in a policy name will still allow Administrators to access.
            var builder = new AuthorizationPolicyBuilder();
            builder.RequireRole(AdministratorRole);
            return builder.Build();
        }

        return policy;
    }
}
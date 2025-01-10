using System.Security.Claims;
using CryptoBank_WebApi.Features.Auth.Domain;
using Microsoft.AspNetCore.Authorization;

namespace CryptoBank_WebApi.Authorization.Requirements;

public class RoleRequirement : IAuthorizationRequirement
{
    public RoleRequirement(UserRole requiredRole)
    {
        RequiredRole = requiredRole;
    }

    public UserRole RequiredRole { get; }
}

public class RoleRequirementHandler : AuthorizationHandler<RoleRequirement>
{
    protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, RoleRequirement requirement)
    {
        if (context.User.HasClaim(ClaimTypes.Role, requirement.RequiredRole.ToString()))
        {
            context.Succeed(requirement);
        }

        return Task.CompletedTask;
    }
}
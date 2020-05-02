using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using XI.Portal.Auth.Contract.Constants;
using XI.Portal.Auth.Migration.AuthorizationRequirements;

namespace XI.Portal.Auth.Migration.AuthorizationHandlers
{
    public class AccessMigrationHandler : AuthorizationHandler<AccessMigration>
    {
        protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, AccessMigration requirement)
        {
            if (context.User.HasClaim(claim => claim.Type == XtremeIdiotsClaimTypes.SeniorAdmin))
                context.Succeed(requirement);

            return Task.CompletedTask;
        }
    }
}
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using XI.Portal.Auth.Contract.Constants;
using XI.Portal.Auth.Users.AuthorizationRequirements;

namespace XI.Portal.Auth.Users.AuthorizationHandlers
{
    public class AccessUsersHandler : AuthorizationHandler<AccessUsers>
    {
        protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, AccessUsers requirement)
        {
            if (context.User.HasClaim(claim => claim.Type == XtremeIdiotsClaimTypes.SeniorAdmin))
                context.Succeed(requirement);

            return Task.CompletedTask;
        }
    }
}
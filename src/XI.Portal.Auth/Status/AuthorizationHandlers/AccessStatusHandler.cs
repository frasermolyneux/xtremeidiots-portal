using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using XI.Portal.Auth.Contract.Constants;
using XI.Portal.Auth.Status.AuthorizationRequirements;

namespace XI.Portal.Auth.Status.AuthorizationHandlers
{
    public class AccessStatusHandler : AuthorizationHandler<AccessStatus>
    {
        protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, AccessStatus requirement)
        {
            if (context.User.HasClaim(claim => claim.Type == XtremeIdiotsClaimTypes.SeniorAdmin))
                context.Succeed(requirement);

            if (context.User.HasClaim(claim => claim.Type == XtremeIdiotsClaimTypes.HeadAdmin))
                context.Succeed(requirement);

            if (context.User.HasClaim(claim => claim.Type == XtremeIdiotsClaimTypes.GameAdmin))
                context.Succeed(requirement);

            return Task.CompletedTask;
        }
    }
}
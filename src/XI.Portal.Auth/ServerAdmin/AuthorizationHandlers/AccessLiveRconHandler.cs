using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using XI.Portal.Auth.Contract.Constants;
using XI.Portal.Auth.ServerAdmin.AuthorizationRequirements;

namespace XI.Portal.Auth.ServerAdmin.AuthorizationHandlers
{
    public class AccessLiveRconHandler : AuthorizationHandler<AccessLiveRcon>
    {
        protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, AccessLiveRcon requirement)
        {
            if (context.User.HasClaim(claim => claim.Type == XtremeIdiotsClaimTypes.SeniorAdmin))
                context.Succeed(requirement);

            if (context.User.HasClaim(claim => claim.Type == XtremeIdiotsClaimTypes.HeadAdmin))
                context.Succeed(requirement);

            if (context.User.HasClaim(claim => claim.Type == XtremeIdiotsClaimTypes.GameAdmin))
                context.Succeed(requirement);

            if (context.User.HasClaim(claim => claim.Type == PortalClaimTypes.LiveRcon))
                context.Succeed(requirement);

            return Task.CompletedTask;
        }
    }
}
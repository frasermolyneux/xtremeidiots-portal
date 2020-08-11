using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using XI.Portal.Auth.Contract.Constants;
using XI.Portal.Auth.ServerAdmin.AuthorizationRequirements;

namespace XI.Portal.Auth.ServerAdmin.AuthorizationHandlers
{
    public class AccessServerAdminHandler : AuthorizationHandler<AccessServerAdmin>
    {
        protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, AccessServerAdmin requirement)
        {
            if (context.User.HasClaim(claim => claim.Type == XtremeIdiotsClaimTypes.SeniorAdmin))
                context.Succeed(requirement);

            if (context.User.HasClaim(claim => claim.Type == XtremeIdiotsClaimTypes.HeadAdmin))
                context.Succeed(requirement);

            if (context.User.HasClaim(claim => claim.Type == XtremeIdiotsClaimTypes.GameAdmin))
                context.Succeed(requirement);

            if (context.User.HasClaim(claim => claim.Type == XtremeIdiotsClaimTypes.Moderator))
                context.Succeed(requirement);

            if (context.User.HasClaim(claim => claim.Type == PortalClaimTypes.ServerAdmin))
                context.Succeed(requirement);

            return Task.CompletedTask;
        }
    }
}
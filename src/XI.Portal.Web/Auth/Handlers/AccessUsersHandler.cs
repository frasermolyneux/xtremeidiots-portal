using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using XI.Portal.Web.Auth.Constants;
using XI.Portal.Web.Auth.Requirements;

namespace XI.Portal.Web.Auth.Handlers
{
    public class AccessUsersHandler : AuthorizationHandler<AccessUsers>
    {
        protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, AccessUsers requirement)
        {
            if (context.User.HasClaim(claim => claim.Type == XtremeIdiotsClaimTypes.SeniorAdmin))
                context.Succeed(requirement);

            if (context.User.HasClaim(claim => claim.Type == XtremeIdiotsClaimTypes.HeadAdmin))
                context.Succeed(requirement);

            return Task.CompletedTask;
        }
    }
}
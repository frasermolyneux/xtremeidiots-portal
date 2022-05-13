using Microsoft.AspNetCore.Authorization;
using System.Linq;
using System.Threading.Tasks;
using XI.Portal.Auth.Contract.Constants;
using XI.Portal.Web.Auth.Requirements;

namespace XI.Portal.Web.Auth.Handlers
{
    public class StatusAuthHandler : IAuthorizationHandler
    {
        public Task HandleAsync(AuthorizationHandlerContext context)
        {
            var pendingRequirements = context.PendingRequirements.ToList();

            foreach (var requirement in pendingRequirements)
            {
                if (requirement is AccessStatus)
                    HandleAccessStatus(context, requirement);
            }

            return Task.CompletedTask;
        }

        private void HandleAccessStatus(AuthorizationHandlerContext context, IAuthorizationRequirement requirement)
        {
            if (context.User.HasClaim(claim => claim.Type == XtremeIdiotsClaimTypes.SeniorAdmin))
                context.Succeed(requirement);

            if (context.User.HasClaim(claim => claim.Type == XtremeIdiotsClaimTypes.HeadAdmin))
                context.Succeed(requirement);

            if (context.User.HasClaim(claim => claim.Type == XtremeIdiotsClaimTypes.GameAdmin))
                context.Succeed(requirement);
        }
    }
}

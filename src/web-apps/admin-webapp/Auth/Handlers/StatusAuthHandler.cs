using Microsoft.AspNetCore.Authorization;
using System.Linq;
using System.Threading.Tasks;
using XtremeIdiots.Portal.AdminWebApp.Auth.Constants;
using XtremeIdiots.Portal.AdminWebApp.Auth.Requirements;

namespace XtremeIdiots.Portal.AdminWebApp.Auth.Handlers
{
    public class StatusAuthHandler : IAuthorizationHandler
    {
        public Task HandleAsync(AuthorizationHandlerContext context)
        {
            var pendingRequirements = context.PendingRequirements.ToList();

            foreach (var requirement in pendingRequirements)
                if (requirement is AccessStatus)
                    HandleAccessStatus(context, requirement);

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

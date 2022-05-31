using Microsoft.AspNetCore.Authorization;
using System.Linq;
using System.Threading.Tasks;
using XtremeIdiots.Portal.AdminWebApp.Auth.Requirements;

namespace XtremeIdiots.Portal.AdminWebApp.Auth.Handlers
{
    public class MapsAuthHandler : IAuthorizationHandler
    {
        public Task HandleAsync(AuthorizationHandlerContext context)
        {
            var pendingRequirements = context.PendingRequirements.ToList();

            foreach (var requirement in pendingRequirements)
                if (requirement is AccessMaps)
                    HandleAccessMaps(context, requirement);

            return Task.CompletedTask;
        }

        private void HandleAccessMaps(AuthorizationHandlerContext context, IAuthorizationRequirement requirement)
        {
            context.Succeed(requirement);
        }
    }
}

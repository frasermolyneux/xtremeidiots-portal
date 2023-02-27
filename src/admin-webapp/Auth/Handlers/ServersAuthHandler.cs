using Microsoft.AspNetCore.Authorization;
using System.Linq;
using System.Threading.Tasks;
using XtremeIdiots.Portal.AdminWebApp.Auth.Requirements;

namespace XtremeIdiots.Portal.AdminWebApp.Auth.Handlers
{
    public class ServersAuthHandler : IAuthorizationHandler
    {
        public Task HandleAsync(AuthorizationHandlerContext context)
        {
            var pendingRequirements = context.PendingRequirements.ToList();

            foreach (var requirement in pendingRequirements)
                if (requirement is AccessServers)
                    HandleAccessServers(context, requirement);

            return Task.CompletedTask;
        }

        private void HandleAccessServers(AuthorizationHandlerContext context, IAuthorizationRequirement requirement)
        {
            context.Succeed(requirement);
        }
    }
}

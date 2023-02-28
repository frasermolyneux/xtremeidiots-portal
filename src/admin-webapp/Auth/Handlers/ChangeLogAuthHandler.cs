using Microsoft.AspNetCore.Authorization;
using System.Linq;
using System.Threading.Tasks;
using XtremeIdiots.Portal.AdminWebApp.Auth.Requirements;

namespace XtremeIdiots.Portal.AdminWebApp.Auth.Handlers
{
    public class ChangeLogAuthHandler : IAuthorizationHandler
    {
        public Task HandleAsync(AuthorizationHandlerContext context)
        {
            var pendingRequirements = context.PendingRequirements.ToList();

            foreach (var requirement in pendingRequirements)
                if (requirement is AccessChangeLog)
                    HandleAccessChangeLog(requirement, context);

            return Task.CompletedTask;
        }

        private void HandleAccessChangeLog(IAuthorizationRequirement requirement, AuthorizationHandlerContext context)
        {
            context.Succeed(requirement);
        }
    }
}

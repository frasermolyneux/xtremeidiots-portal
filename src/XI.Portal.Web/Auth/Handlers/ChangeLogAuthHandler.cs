using Microsoft.AspNetCore.Authorization;
using System.Linq;
using System.Threading.Tasks;
using XI.Portal.Web.Auth.Requirements;

namespace XI.Portal.Web.Auth.Handlers
{
    public class ChangeLogAuthHandler : IAuthorizationHandler
    {
        public Task HandleAsync(AuthorizationHandlerContext context)
        {
            var pendingRequirements = context.PendingRequirements.ToList();

            foreach (var requirement in pendingRequirements)
            {
                if (requirement is AccessChangeLog)
                    HandleAccessChangeLog(requirement, context);
            }

            return Task.CompletedTask;
        }

        private void HandleAccessChangeLog(IAuthorizationRequirement requirement, AuthorizationHandlerContext context)
        {
            context.Succeed(requirement);
        }
    }
}

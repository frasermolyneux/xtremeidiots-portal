using Microsoft.AspNetCore.Authorization;
using System.Linq;
using System.Threading.Tasks;
using XI.Portal.Web.Auth.Requirements;

namespace XI.Portal.Web.Auth.Handlers
{
    public class HomeAuthHandler : IAuthorizationHandler
    {
        public Task HandleAsync(AuthorizationHandlerContext context)
        {
            var pendingRequirements = context.PendingRequirements.ToList();

            foreach (var requirement in pendingRequirements)
            {
                if (requirement is AccessHome)
                    HandleAccessHome(context, requirement);
            }

            return Task.CompletedTask;
        }

        private void HandleAccessHome(AuthorizationHandlerContext context, IAuthorizationRequirement requirement)
        {
            context.Succeed(requirement);
        }
    }
}

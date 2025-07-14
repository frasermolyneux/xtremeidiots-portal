using Microsoft.AspNetCore.Authorization;
using System.Linq;
using System.Threading.Tasks;
using XtremeIdiots.Portal.Web.Auth.Requirements;

namespace XtremeIdiots.Portal.Web.Auth.Handlers
{
    public class ProfileAuthHandler : IAuthorizationHandler
    {
        public Task HandleAsync(AuthorizationHandlerContext context)
        {
            var pendingRequirements = context.PendingRequirements.ToList();

            foreach (var requirement in pendingRequirements)
                if (requirement is AccessProfile)
                    HandleAccessProfile(context, requirement);

            return Task.CompletedTask;
        }

        private void HandleAccessProfile(AuthorizationHandlerContext context, IAuthorizationRequirement requirement)
        {
            // Allow access to any authenticated user for their own profile
            if (context.User.Identity?.IsAuthenticated == true)
            {
                context.Succeed(requirement);
            }
        }
    }
}

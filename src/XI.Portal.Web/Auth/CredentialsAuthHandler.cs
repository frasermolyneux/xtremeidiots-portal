using Microsoft.AspNetCore.Authorization;
using System.Linq;
using System.Threading.Tasks;
using XI.Portal.Auth.Contract.Constants;

namespace XI.Portal.Web.Auth
{
    public class CredentialsAuthHandler : IAuthorizationHandler
    {
        public Task HandleAsync(AuthorizationHandlerContext context)
        {
            var pendingRequirements = context.PendingRequirements.ToList();

            foreach (var requirement in pendingRequirements)
            {
                if (requirement is AccessCredentials)
                    HandleAccessCredentials(requirement, context);
            }

            return Task.CompletedTask;
        }

        private void HandleAccessCredentials(IAuthorizationRequirement requirement, AuthorizationHandlerContext context)
        {
            if (context.User.HasClaim(claim => claim.Type == XtremeIdiotsClaimTypes.SeniorAdmin))
                context.Succeed(requirement);

            if (context.User.HasClaim(claim => claim.Type == XtremeIdiotsClaimTypes.HeadAdmin))
                context.Succeed(requirement);

            if (context.User.HasClaim(claim => claim.Type == XtremeIdiotsClaimTypes.GameAdmin))
                context.Succeed(requirement);

            if (context.User.HasClaim(claim => claim.Type == PortalClaimTypes.RconCredentials))
                context.Succeed(requirement);

            if (context.User.HasClaim(claim => claim.Type == PortalClaimTypes.FtpCredentials))
                context.Succeed(requirement);
        }
    }
}

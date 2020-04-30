using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using XI.Portal.Auth.Contract.Constants;
using XI.Portal.Auth.Credentials.AuthorizationRequirements;

namespace XI.Portal.Auth.Credentials.AuthorizationHandlers
{
    public class AccessCredentialsHandler : AuthorizationHandler<AccessCredentials>
    {
        protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, AccessCredentials requirement)
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

            return Task.CompletedTask;
        }
    }
}
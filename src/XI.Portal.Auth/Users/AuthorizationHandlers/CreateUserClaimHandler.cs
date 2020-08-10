using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using XI.Portal.Auth.Contract.Constants;
using XI.Portal.Auth.Users.AuthorizationRequirements;
using XI.Portal.Servers.Dto;

namespace XI.Portal.Auth.Users.AuthorizationHandlers
{
    public class CreateUserClaimHandler : AuthorizationHandler<CreateUserClaim, GameServerDto>
    {
        protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, CreateUserClaim requirement, GameServerDto resource)
        {
            if (context.User.HasClaim(claim => claim.Type == XtremeIdiotsClaimTypes.SeniorAdmin))
                context.Succeed(requirement);

            if (context.User.HasClaim(claim => claim.Type == XtremeIdiotsClaimTypes.HeadAdmin && claim.Value == resource.GameType.ToString()))
                context.Succeed(requirement);

            return Task.CompletedTask;
        }
    }
}
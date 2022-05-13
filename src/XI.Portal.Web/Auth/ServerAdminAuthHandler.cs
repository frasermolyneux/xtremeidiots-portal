using Microsoft.AspNetCore.Authorization;
using System.Linq;
using System.Threading.Tasks;
using XI.Portal.Auth.Contract.Constants;
using XtremeIdiots.Portal.RepositoryApi.Abstractions.NetStandard.Constants;

namespace XI.Portal.Web.Auth
{
    public class ServerAdminAuthHandler : IAuthorizationHandler
    {
        public Task HandleAsync(AuthorizationHandlerContext context)
        {
            var pendingRequirements = context.PendingRequirements.ToList();

            foreach (var requirement in pendingRequirements)
            {
                if (requirement is AccessLiveRcon)
                    HandleAccessLiveRcon(context, requirement);

                if (requirement is AccessServerAdmin)
                    HandleAccessServerAdmin(context, requirement);

                if (requirement is ViewGameChatLog)
                    HandleViewGameChatLog(context, requirement);

                if (requirement is ViewGlobalChatLog)
                    HandleViewGlobalChatLog(context, requirement);

                if (requirement is ViewLiveRcon)
                    HandleViewLiveRcon(context, requirement);

                if (requirement is ViewServerChatLog)
                    HandleViewServerChatLog(context, requirement);
            }

            return Task.CompletedTask;
        }

        private void HandleViewServerChatLog(AuthorizationHandlerContext context, IAuthorizationRequirement requirement)
        {
            if (context.User.HasClaim(claim => claim.Type == XtremeIdiotsClaimTypes.SeniorAdmin))
                context.Succeed(requirement);

            if (context.Resource is GameType)
            {
                var gameType = (GameType)context.Resource;

                if (context.User.HasClaim(XtremeIdiotsClaimTypes.HeadAdmin, gameType.ToString()))
                    context.Succeed(requirement);

                if (context.User.HasClaim(XtremeIdiotsClaimTypes.GameAdmin, gameType.ToString()))
                    context.Succeed(requirement);

                if (context.User.HasClaim(XtremeIdiotsClaimTypes.Moderator, gameType.ToString()))
                    context.Succeed(requirement);
            }
        }

        private void HandleViewLiveRcon(AuthorizationHandlerContext context, IAuthorizationRequirement requirement)
        {
            if (context.User.HasClaim(claim => claim.Type == XtremeIdiotsClaimTypes.SeniorAdmin))
                context.Succeed(requirement);

            if (context.Resource is GameType)
            {
                var gameType = (GameType)context.Resource;

                if (context.User.HasClaim(XtremeIdiotsClaimTypes.HeadAdmin, gameType.ToString()))
                    context.Succeed(requirement);

                if (context.User.HasClaim(XtremeIdiotsClaimTypes.GameAdmin, gameType.ToString()))
                    context.Succeed(requirement);

                if (context.User.HasClaim(PortalClaimTypes.LiveRcon, gameType.ToString()))
                    context.Succeed(requirement);
            }
        }

        private void HandleViewGlobalChatLog(AuthorizationHandlerContext context, IAuthorizationRequirement requirement)
        {
            if (context.User.HasClaim(claim => claim.Type == XtremeIdiotsClaimTypes.SeniorAdmin))
                context.Succeed(requirement);

            if (context.User.HasClaim(claim => claim.Type == XtremeIdiotsClaimTypes.HeadAdmin))
                context.Succeed(requirement);

            if (context.User.HasClaim(claim => claim.Type == XtremeIdiotsClaimTypes.GameAdmin))
                context.Succeed(requirement);
        }

        private void HandleViewGameChatLog(AuthorizationHandlerContext context, IAuthorizationRequirement requirement)
        {
            if (context.User.HasClaim(claim => claim.Type == XtremeIdiotsClaimTypes.SeniorAdmin))
                context.Succeed(requirement);

            if (context.Resource is GameType)
            {
                var gameType = (GameType)context.Resource;

                if (context.User.HasClaim(XtremeIdiotsClaimTypes.HeadAdmin, gameType.ToString()))
                    context.Succeed(requirement);

                if (context.User.HasClaim(XtremeIdiotsClaimTypes.GameAdmin, gameType.ToString()))
                    context.Succeed(requirement);
            }
        }

        private void HandleAccessServerAdmin(AuthorizationHandlerContext context, IAuthorizationRequirement requirement)
        {
            if (context.User.HasClaim(claim => claim.Type == XtremeIdiotsClaimTypes.SeniorAdmin))
                context.Succeed(requirement);

            if (context.User.HasClaim(claim => claim.Type == XtremeIdiotsClaimTypes.HeadAdmin))
                context.Succeed(requirement);

            if (context.User.HasClaim(claim => claim.Type == XtremeIdiotsClaimTypes.GameAdmin))
                context.Succeed(requirement);

            if (context.User.HasClaim(claim => claim.Type == XtremeIdiotsClaimTypes.Moderator))
                context.Succeed(requirement);

            if (context.User.HasClaim(claim => claim.Type == PortalClaimTypes.ServerAdmin))
                context.Succeed(requirement);
        }

        private void HandleAccessLiveRcon(AuthorizationHandlerContext context, IAuthorizationRequirement requirement)
        {
            if (context.User.HasClaim(claim => claim.Type == XtremeIdiotsClaimTypes.SeniorAdmin))
                context.Succeed(requirement);

            if (context.User.HasClaim(claim => claim.Type == XtremeIdiotsClaimTypes.HeadAdmin))
                context.Succeed(requirement);

            if (context.User.HasClaim(claim => claim.Type == XtremeIdiotsClaimTypes.GameAdmin))
                context.Succeed(requirement);

            if (context.User.HasClaim(claim => claim.Type == PortalClaimTypes.LiveRcon))
                context.Succeed(requirement);
        }
    }
}

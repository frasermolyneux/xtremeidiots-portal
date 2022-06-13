using Microsoft.AspNetCore.Authorization;

using XtremeIdiots.Portal.AdminWebApp.Auth.Constants;
using XtremeIdiots.Portal.AdminWebApp.Auth.Requirements;
using XtremeIdiots.Portal.RepositoryApi.Abstractions.Constants;

namespace XtremeIdiots.Portal.AdminWebApp.Auth.Handlers
{
    public class GameServersAuthHandler : IAuthorizationHandler
    {
        public Task HandleAsync(AuthorizationHandlerContext context)
        {
            var pendingRequirements = context.PendingRequirements.ToList();

            foreach (var requirement in pendingRequirements)
            {
                if (requirement is AccessGameServers)
                    HandleAccessGameServers(context, requirement);

                if (requirement is CreateGameServer)
                    HandleCreateGameServer(context, requirement);

                if (requirement is DeleteGameServer)
                    HandleDeleteGameServer(context, requirement);

                if (requirement is EditGameServerFtp)
                    HandleEditGameServerFtp(context, requirement);

                if (requirement is EditGameServer)
                    HandleEditGameServer(context, requirement);

                if (requirement is EditGameServerRcon)
                    HandleEditGameServerRcon(context, requirement);

                if (requirement is ViewFtpCredential)
                    HandleViewFtpCredential(context, requirement);

                if (requirement is ViewGameServer)
                    HandleViewGameServer(context, requirement);

                if (requirement is ViewRconCredential)
                    HandleViewRconCredential(context, requirement);
            }

            return Task.CompletedTask;
        }

        private static void HandleViewRconCredential(AuthorizationHandlerContext context, IAuthorizationRequirement requirement)
        {
            var requiredClaims = new string[] { XtremeIdiotsClaimTypes.SeniorAdmin };

            if (context.User.Claims.Any(claim => requiredClaims.Contains(claim.Type)))
                context.Succeed(requirement);

            if (context.Resource is Tuple<GameType, Guid>)
            {
                var (gameType, gameServerId) = (Tuple<GameType, Guid>)context.Resource;

                if (context.User.HasClaim(XtremeIdiotsClaimTypes.HeadAdmin, gameType.ToString()))
                    context.Succeed(requirement);

                if (context.User.HasClaim(XtremeIdiotsClaimTypes.GameAdmin, gameType.ToString()))
                    context.Succeed(requirement);

                if (context.User.HasClaim(PortalClaimTypes.RconCredentials, gameServerId.ToString()))
                    context.Succeed(requirement);
            }
        }

        private static void HandleViewGameServer(AuthorizationHandlerContext context, IAuthorizationRequirement requirement)
        {
            var requiredClaims = new string[] { XtremeIdiotsClaimTypes.SeniorAdmin };

            if (context.User.Claims.Any(claim => requiredClaims.Contains(claim.Type)))
                context.Succeed(requirement);

            if (context.Resource is GameType)
            {
                if (context.User.HasClaim(XtremeIdiotsClaimTypes.HeadAdmin, context.Resource.ToString()))
                    context.Succeed(requirement);

                if (context.User.HasClaim(PortalClaimTypes.GameServer, context.Resource.ToString()))
                    context.Succeed(requirement);
            }
        }

        private static void HandleViewFtpCredential(AuthorizationHandlerContext context, IAuthorizationRequirement requirement)
        {
            var requiredClaims = new string[] { XtremeIdiotsClaimTypes.SeniorAdmin };

            if (context.User.Claims.Any(claim => requiredClaims.Contains(claim.Type)))
                context.Succeed(requirement);

            if (context.Resource is Tuple<GameType, Guid>)
            {
                var (gameType, gameServerId) = (Tuple<GameType, Guid>)context.Resource;

                if (context.User.HasClaim(claim => claim.Type == XtremeIdiotsClaimTypes.HeadAdmin && claim.Value == gameType.ToString()))
                    context.Succeed(requirement);

                if (context.User.HasClaim(PortalClaimTypes.FtpCredentials, gameServerId.ToString()))
                    context.Succeed(requirement);
            }
        }

        private static void HandleEditGameServerRcon(AuthorizationHandlerContext context, IAuthorizationRequirement requirement)
        {
            var requiredClaims = new string[] { XtremeIdiotsClaimTypes.SeniorAdmin };

            if (context.User.Claims.Any(claim => requiredClaims.Contains(claim.Type)))
                context.Succeed(requirement);

            if (context.Resource is GameType)
                if (context.User.HasClaim(XtremeIdiotsClaimTypes.HeadAdmin, context.Resource.ToString()))
                    context.Succeed(requirement);
        }

        private static void HandleEditGameServer(AuthorizationHandlerContext context, IAuthorizationRequirement requirement)
        {
            var requiredClaims = new string[] { XtremeIdiotsClaimTypes.SeniorAdmin };

            if (context.User.Claims.Any(claim => requiredClaims.Contains(claim.Type)))
                context.Succeed(requirement);

            if (context.Resource is GameType)
            {
                if (context.User.HasClaim(XtremeIdiotsClaimTypes.HeadAdmin, context.Resource.ToString()))
                    context.Succeed(requirement);

                if (context.User.HasClaim(PortalClaimTypes.GameServer, context.Resource.ToString()))
                    context.Succeed(requirement);
            }
        }

        private static void HandleEditGameServerFtp(AuthorizationHandlerContext context, IAuthorizationRequirement requirement)
        {
            var requiredClaims = new string[] { XtremeIdiotsClaimTypes.SeniorAdmin };

            if (context.User.Claims.Any(claim => requiredClaims.Contains(claim.Type)))
                context.Succeed(requirement);

            if (context.Resource is GameType)
                if (context.User.HasClaim(XtremeIdiotsClaimTypes.HeadAdmin, context.Resource.ToString()))
                    context.Succeed(requirement);
        }

        private static void HandleDeleteGameServer(AuthorizationHandlerContext context, IAuthorizationRequirement requirement)
        {
            var requiredClaims = new string[] { XtremeIdiotsClaimTypes.SeniorAdmin };

            if (context.User.Claims.Any(claim => requiredClaims.Contains(claim.Type)))
                context.Succeed(requirement);
        }

        private static void HandleCreateGameServer(AuthorizationHandlerContext context, IAuthorizationRequirement requirement)
        {
            var requiredClaims = new string[] { XtremeIdiotsClaimTypes.SeniorAdmin };

            if (context.User.Claims.Any(claim => requiredClaims.Contains(claim.Type)))
                context.Succeed(requirement);

            if (context.Resource is GameType)
                if (context.User.HasClaim(XtremeIdiotsClaimTypes.HeadAdmin, context.Resource.ToString()))
                    context.Succeed(requirement);
        }

        private static void HandleAccessGameServers(AuthorizationHandlerContext context, IAuthorizationRequirement requirement)
        {
            var requiredClaims = new string[] { XtremeIdiotsClaimTypes.SeniorAdmin, XtremeIdiotsClaimTypes.HeadAdmin, PortalClaimTypes.GameServer };

            if (context.User.Claims.Any(claim => requiredClaims.Contains(claim.Type)))
                context.Succeed(requirement);
        }
    }
}

using Microsoft.AspNetCore.Authorization;

using XtremeIdiots.Portal.Web.Auth.Requirements;
using XtremeIdiots.Portal.Repository.Abstractions.Constants.V1;

namespace XtremeIdiots.Portal.Web.Auth.Handlers
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
            var requiredClaims = new string[] { UserProfileClaimType.SeniorAdmin };

            if (context.User.Claims.Any(claim => requiredClaims.Contains(claim.Type)))
                context.Succeed(requirement);

            if (context.Resource is Tuple<GameType, Guid>)
            {
                var (gameType, gameServerId) = (Tuple<GameType, Guid>)context.Resource;

                if (context.User.HasClaim(UserProfileClaimType.HeadAdmin, gameType.ToString()))
                    context.Succeed(requirement);

                if (context.User.HasClaim(UserProfileClaimType.GameAdmin, gameType.ToString()))
                    context.Succeed(requirement);

                if (context.User.HasClaim(UserProfileClaimType.RconCredentials, gameServerId.ToString()))
                    context.Succeed(requirement);
            }
        }

        private static void HandleViewGameServer(AuthorizationHandlerContext context, IAuthorizationRequirement requirement)
        {
            var requiredClaims = new string[] { UserProfileClaimType.SeniorAdmin };

            if (context.User.Claims.Any(claim => requiredClaims.Contains(claim.Type)))
                context.Succeed(requirement);

            if (context.Resource is GameType)
            {
                if (context.User.HasClaim(UserProfileClaimType.HeadAdmin, context.Resource.ToString()))
                    context.Succeed(requirement);

                if (context.User.HasClaim(UserProfileClaimType.GameServer, context.Resource.ToString()))
                    context.Succeed(requirement);
            }
        }

        private static void HandleViewFtpCredential(AuthorizationHandlerContext context, IAuthorizationRequirement requirement)
        {
            var requiredClaims = new string[] { UserProfileClaimType.SeniorAdmin };

            if (context.User.Claims.Any(claim => requiredClaims.Contains(claim.Type)))
                context.Succeed(requirement);

            if (context.Resource is Tuple<GameType, Guid>)
            {
                var (gameType, gameServerId) = (Tuple<GameType, Guid>)context.Resource;

                if (context.User.HasClaim(claim => claim.Type == UserProfileClaimType.HeadAdmin && claim.Value == gameType.ToString()))
                    context.Succeed(requirement);

                if (context.User.HasClaim(UserProfileClaimType.FtpCredentials, gameServerId.ToString()))
                    context.Succeed(requirement);
            }
        }

        private static void HandleEditGameServerRcon(AuthorizationHandlerContext context, IAuthorizationRequirement requirement)
        {
            var requiredClaims = new string[] { UserProfileClaimType.SeniorAdmin };

            if (context.User.Claims.Any(claim => requiredClaims.Contains(claim.Type)))
                context.Succeed(requirement);

            if (context.Resource is GameType)
                if (context.User.HasClaim(UserProfileClaimType.HeadAdmin, context.Resource.ToString()))
                    context.Succeed(requirement);
        }

        private static void HandleEditGameServer(AuthorizationHandlerContext context, IAuthorizationRequirement requirement)
        {
            var requiredClaims = new string[] { UserProfileClaimType.SeniorAdmin };

            if (context.User.Claims.Any(claim => requiredClaims.Contains(claim.Type)))
                context.Succeed(requirement);

            if (context.Resource is GameType)
            {
                if (context.User.HasClaim(UserProfileClaimType.HeadAdmin, context.Resource.ToString()))
                    context.Succeed(requirement);

                if (context.User.HasClaim(UserProfileClaimType.GameServer, context.Resource.ToString()))
                    context.Succeed(requirement);
            }
        }

        private static void HandleEditGameServerFtp(AuthorizationHandlerContext context, IAuthorizationRequirement requirement)
        {
            var requiredClaims = new string[] { UserProfileClaimType.SeniorAdmin };

            if (context.User.Claims.Any(claim => requiredClaims.Contains(claim.Type)))
                context.Succeed(requirement);

            if (context.Resource is GameType)
                if (context.User.HasClaim(UserProfileClaimType.HeadAdmin, context.Resource.ToString()))
                    context.Succeed(requirement);
        }

        private static void HandleDeleteGameServer(AuthorizationHandlerContext context, IAuthorizationRequirement requirement)
        {
            var requiredClaims = new string[] { UserProfileClaimType.SeniorAdmin };

            if (context.User.Claims.Any(claim => requiredClaims.Contains(claim.Type)))
                context.Succeed(requirement);
        }

        private static void HandleCreateGameServer(AuthorizationHandlerContext context, IAuthorizationRequirement requirement)
        {
            var requiredClaims = new string[] { UserProfileClaimType.SeniorAdmin };

            if (context.User.Claims.Any(claim => requiredClaims.Contains(claim.Type)))
                context.Succeed(requirement);

            if (context.Resource is GameType)
                if (context.User.HasClaim(UserProfileClaimType.HeadAdmin, context.Resource.ToString()))
                    context.Succeed(requirement);
        }

        private static void HandleAccessGameServers(AuthorizationHandlerContext context, IAuthorizationRequirement requirement)
        {
            var requiredClaims = new string[] { UserProfileClaimType.SeniorAdmin, UserProfileClaimType.HeadAdmin, UserProfileClaimType.GameServer };

            if (context.User.Claims.Any(claim => requiredClaims.Contains(claim.Type)))
                context.Succeed(requirement);
        }
    }
}

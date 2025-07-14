using Microsoft.AspNetCore.Authorization;
using XtremeIdiots.Portal.Repository.Abstractions.Constants.V1;
using XtremeIdiots.Portal.Web.Auth.Requirements;

namespace XtremeIdiots.Portal.Web.Auth.Handlers
{
    /// <summary>
    /// Handles authorization for game server operations including access, creation, deletion, editing, and credential management.
    /// Supports senior admin, head admin, game admin, and server-specific permission levels.
    /// </summary>
    public class GameServersAuthHandler : IAuthorizationHandler
    {
        /// <summary>
        /// Handles authorization requirements for game server operations.
        /// </summary>
        /// <param name="context">The authorization context containing user claims and resource information.</param>
        /// <returns>A completed task.</returns>
        public Task HandleAsync(AuthorizationHandlerContext context)
        {
            var pendingRequirements = context.PendingRequirements.ToList();

            foreach (var requirement in pendingRequirements)
            {
                switch (requirement)
                {
                    case AccessGameServers:
                        HandleAccessGameServers(context, requirement);
                        break;
                    case CreateGameServer:
                        HandleCreateGameServer(context, requirement);
                        break;
                    case DeleteGameServer:
                        HandleDeleteGameServer(context, requirement);
                        break;
                    case EditGameServerFtp:
                        HandleEditGameServerFtp(context, requirement);
                        break;
                    case EditGameServer:
                        HandleEditGameServer(context, requirement);
                        break;
                    case EditGameServerRcon:
                        HandleEditGameServerRcon(context, requirement);
                        break;
                    case ViewFtpCredential:
                        HandleViewFtpCredential(context, requirement);
                        break;
                    case ViewGameServer:
                        HandleViewGameServer(context, requirement);
                        break;
                    case ViewRconCredential:
                        HandleViewRconCredential(context, requirement);
                        break;
                }
            }

            return Task.CompletedTask;
        }

        #region Authorization Handlers

        /// <summary>
        /// Handles authorization for accessing game servers.
        /// Allows senior admins, head admins, or users with game server access.
        /// </summary>
        /// <param name="context">The authorization context.</param>
        /// <param name="requirement">The access game servers requirement.</param>
        private static void HandleAccessGameServers(AuthorizationHandlerContext context, IAuthorizationRequirement requirement)
        {
            BaseAuthorizationHelper.CheckClaimTypes(context, requirement, BaseAuthorizationHelper.ClaimGroups.GameServerAccessLevels);
        }

        /// <summary>
        /// Handles authorization for creating game servers.
        /// Allows senior admins or head admins for the game type.
        /// </summary>
        /// <param name="context">The authorization context.</param>
        /// <param name="requirement">The create game server requirement.</param>
        private static void HandleCreateGameServer(AuthorizationHandlerContext context, IAuthorizationRequirement requirement)
        {
            BaseAuthorizationHelper.CheckSeniorOrHeadAdminAccessWithResource(context, requirement);
        }

        /// <summary>
        /// Handles authorization for deleting game servers.
        /// Only allows senior admins.
        /// </summary>
        /// <param name="context">The authorization context.</param>
        /// <param name="requirement">The delete game server requirement.</param>
        private static void HandleDeleteGameServer(AuthorizationHandlerContext context, IAuthorizationRequirement requirement)
        {
            BaseAuthorizationHelper.CheckSeniorAdminAccess(context, requirement);
        }

        /// <summary>
        /// Handles authorization for editing game server FTP settings.
        /// Allows senior admins or head admins for the game type.
        /// </summary>
        /// <param name="context">The authorization context.</param>
        /// <param name="requirement">The edit game server FTP requirement.</param>
        private static void HandleEditGameServerFtp(AuthorizationHandlerContext context, IAuthorizationRequirement requirement)
        {
            BaseAuthorizationHelper.CheckSeniorOrHeadAdminAccessWithResource(context, requirement);
        }

        /// <summary>
        /// Handles authorization for editing game servers.
        /// Allows senior admins, head admins for the game type, or users with game server access.
        /// </summary>
        /// <param name="context">The authorization context.</param>
        /// <param name="requirement">The edit game server requirement.</param>
        private static void HandleEditGameServer(AuthorizationHandlerContext context, IAuthorizationRequirement requirement)
        {
            BaseAuthorizationHelper.CheckSeniorAdminAccess(context, requirement);

            if (context.Resource is GameType gameType)
            {
                BaseAuthorizationHelper.CheckCombinedGameServerAccess(context, requirement, gameType);
            }
        }

        /// <summary>
        /// Handles authorization for editing game server RCON settings.
        /// Allows senior admins or head admins for the game type.
        /// </summary>
        /// <param name="context">The authorization context.</param>
        /// <param name="requirement">The edit game server RCON requirement.</param>
        private static void HandleEditGameServerRcon(AuthorizationHandlerContext context, IAuthorizationRequirement requirement)
        {
            BaseAuthorizationHelper.CheckSeniorOrHeadAdminAccessWithResource(context, requirement);
        }

        /// <summary>
        /// Handles authorization for viewing FTP credentials.
        /// Allows senior admins, head admins for the game type, or users with FTP credentials access.
        /// </summary>
        /// <param name="context">The authorization context.</param>
        /// <param name="requirement">The view FTP credential requirement.</param>
        private static void HandleViewFtpCredential(AuthorizationHandlerContext context, IAuthorizationRequirement requirement)
        {
            BaseAuthorizationHelper.CheckSeniorAdminAccess(context, requirement);

            if (context.Resource is Tuple<GameType, Guid> resource)
            {
                var (gameType, gameServerId) = resource;
                BaseAuthorizationHelper.CheckHeadAdminAccess(context, requirement, gameType);
                BaseAuthorizationHelper.CheckFtpCredentialsAccess(context, requirement, gameServerId);
            }
        }

        /// <summary>
        /// Handles authorization for viewing game servers.
        /// Allows senior admins, head admins for the game type, or users with game server access.
        /// </summary>
        /// <param name="context">The authorization context.</param>
        /// <param name="requirement">The view game server requirement.</param>
        private static void HandleViewGameServer(AuthorizationHandlerContext context, IAuthorizationRequirement requirement)
        {
            BaseAuthorizationHelper.CheckSeniorAdminAccess(context, requirement);

            if (context.Resource is GameType gameType)
            {
                BaseAuthorizationHelper.CheckCombinedGameServerAccess(context, requirement, gameType);
            }
        }

        /// <summary>
        /// Handles authorization for viewing RCON credentials.
        /// Allows senior admins, head admins/game admins for the game type, or users with RCON credentials access.
        /// </summary>
        /// <param name="context">The authorization context.</param>
        /// <param name="requirement">The view RCON credential requirement.</param>
        private static void HandleViewRconCredential(AuthorizationHandlerContext context, IAuthorizationRequirement requirement)
        {
            BaseAuthorizationHelper.CheckSeniorOrLiveRconAccessWithResource(context, requirement);
        }

        #endregion
    }
}

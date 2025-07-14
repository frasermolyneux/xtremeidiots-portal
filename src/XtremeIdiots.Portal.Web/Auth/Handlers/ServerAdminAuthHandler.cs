using Microsoft.AspNetCore.Authorization;
using XtremeIdiots.Portal.Repository.Abstractions.Constants.V1;
using XtremeIdiots.Portal.Web.Auth.Requirements;

namespace XtremeIdiots.Portal.Web.Auth.Handlers
{
    /// <summary>
    /// Handles authorization for server administration operations including RCON access, chat logs, and map management.
    /// Supports various permission levels including server admin, live RCON, and game-specific permissions.
    /// </summary>
    public class ServerAdminAuthHandler : IAuthorizationHandler
    {
        /// <summary>
        /// Handles authorization requirements for server administration operations.
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
                    case AccessLiveRcon:
                        HandleAccessLiveRcon(context, requirement);
                        break;
                    case AccessServerAdmin:
                        HandleAccessServerAdmin(context, requirement);
                        break;
                    case ViewGameChatLog:
                        HandleViewGameChatLog(context, requirement);
                        break;
                    case ViewGlobalChatLog:
                        HandleViewGlobalChatLog(context, requirement);
                        break;
                    case ViewLiveRcon:
                        HandleViewLiveRcon(context, requirement);
                        break;
                    case ViewServerChatLog:
                        HandleViewServerChatLog(context, requirement);
                        break;
                    case ManageMaps:
                        HandleManageMaps(context, requirement);
                        break;
                    case AccessMapManagerController:
                        HandleAccessMapManagerController(context, requirement);
                        break;
                    case PushMapToRemote:
                        HandlePushMapToRemote(context, requirement);
                        break;
                    case DeleteMapFromHost:
                        HandleDeleteMapFromHost(context, requirement);
                        break;
                    case LockChatMessages:
                        HandleLockChatMessages(context, requirement);
                        break;
                }
            }

            return Task.CompletedTask;
        }

        #region Authorization Handlers

        /// <summary>
        /// Handles authorization for accessing live RCON.
        /// Allows admins excluding moderators and users with live RCON access.
        /// </summary>
        /// <param name="context">The authorization context.</param>
        /// <param name="requirement">The access live RCON requirement.</param>
        private static void HandleAccessLiveRcon(AuthorizationHandlerContext context, IAuthorizationRequirement requirement)
        {
            BaseAuthorizationHelper.CheckClaimTypes(context, requirement, BaseAuthorizationHelper.ClaimGroups.LiveRconAccessLevels);
        }

        /// <summary>
        /// Handles authorization for accessing server admin functions.
        /// Allows all admin levels including moderators and server admins.
        /// </summary>
        /// <param name="context">The authorization context.</param>
        /// <param name="requirement">The access server admin requirement.</param>
        private static void HandleAccessServerAdmin(AuthorizationHandlerContext context, IAuthorizationRequirement requirement)
        {
            BaseAuthorizationHelper.CheckClaimTypes(context, requirement, BaseAuthorizationHelper.ClaimGroups.ServerAdminAccessLevels);
        }

        /// <summary>
        /// Handles authorization for viewing game-specific chat logs.
        /// Allows senior admins, head admins, or game admins for the specific game type.
        /// </summary>
        /// <param name="context">The authorization context.</param>
        /// <param name="requirement">The view game chat log requirement.</param>
        private static void HandleViewGameChatLog(AuthorizationHandlerContext context, IAuthorizationRequirement requirement)
        {
            BaseAuthorizationHelper.CheckSeniorOrGameAdminAccessWithResource(context, requirement);
        }

        /// <summary>
        /// Handles authorization for viewing global chat logs.
        /// Allows admin levels excluding moderators.
        /// </summary>
        /// <param name="context">The authorization context.</param>
        /// <param name="requirement">The view global chat log requirement.</param>
        private static void HandleViewGlobalChatLog(AuthorizationHandlerContext context, IAuthorizationRequirement requirement)
        {
            BaseAuthorizationHelper.CheckClaimTypes(context, requirement, BaseAuthorizationHelper.ClaimGroups.AdminLevelsExcludingModerators);
        }

        /// <summary>
        /// Handles authorization for viewing live RCON.
        /// Allows senior admins, head admins, game admins, or users with live RCON access for the game type.
        /// </summary>
        /// <param name="context">The authorization context.</param>
        /// <param name="requirement">The view live RCON requirement.</param>
        private static void HandleViewLiveRcon(AuthorizationHandlerContext context, IAuthorizationRequirement requirement)
        {
            BaseAuthorizationHelper.CheckSeniorOrLiveRconAccessWithResource(context, requirement);
        }

        /// <summary>
        /// Handles authorization for viewing server-specific chat logs.
        /// Allows all admin levels including moderators for the specific game type.
        /// </summary>
        /// <param name="context">The authorization context.</param>
        /// <param name="requirement">The view server chat log requirement.</param>
        private static void HandleViewServerChatLog(AuthorizationHandlerContext context, IAuthorizationRequirement requirement)
        {
            BaseAuthorizationHelper.CheckSeniorOrMultipleGameAccessWithResource(context, requirement);
        }

        /// <summary>
        /// Handles authorization for managing maps.
        /// Allows senior admins or head admins.
        /// </summary>
        /// <param name="context">The authorization context.</param>
        /// <param name="requirement">The manage maps requirement.</param>
        private static void HandleManageMaps(AuthorizationHandlerContext context, IAuthorizationRequirement requirement)
        {
            BaseAuthorizationHelper.CheckClaimTypes(context, requirement, BaseAuthorizationHelper.ClaimGroups.SeniorAndHeadAdminOnly);
        }

        /// <summary>
        /// Handles authorization for accessing the map manager controller.
        /// Allows senior admins or head admins.
        /// </summary>
        /// <param name="context">The authorization context.</param>
        /// <param name="requirement">The access map manager controller requirement.</param>
        private static void HandleAccessMapManagerController(AuthorizationHandlerContext context, IAuthorizationRequirement requirement)
        {
            BaseAuthorizationHelper.CheckClaimTypes(context, requirement, BaseAuthorizationHelper.ClaimGroups.SeniorAndHeadAdminOnly);
        }

        /// <summary>
        /// Handles authorization for pushing maps to remote servers.
        /// Allows senior admins or head admins.
        /// </summary>
        /// <param name="context">The authorization context.</param>
        /// <param name="requirement">The push map to remote requirement.</param>
        private static void HandlePushMapToRemote(AuthorizationHandlerContext context, IAuthorizationRequirement requirement)
        {
            BaseAuthorizationHelper.CheckClaimTypes(context, requirement, BaseAuthorizationHelper.ClaimGroups.SeniorAndHeadAdminOnly);
        }

        /// <summary>
        /// Handles authorization for deleting maps from host servers.
        /// Allows senior admins or head admins.
        /// </summary>
        /// <param name="context">The authorization context.</param>
        /// <param name="requirement">The delete map from host requirement.</param>
        private static void HandleDeleteMapFromHost(AuthorizationHandlerContext context, IAuthorizationRequirement requirement)
        {
            BaseAuthorizationHelper.CheckClaimTypes(context, requirement, BaseAuthorizationHelper.ClaimGroups.SeniorAndHeadAdminOnly);
        }

        /// <summary>
        /// Handles authorization for locking chat messages.
        /// Allows admin levels excluding moderators, or moderators for their specific game type.
        /// </summary>
        /// <param name="context">The authorization context.</param>
        /// <param name="requirement">The lock chat messages requirement.</param>
        private static void HandleLockChatMessages(AuthorizationHandlerContext context, IAuthorizationRequirement requirement)
        {
            BaseAuthorizationHelper.CheckClaimTypes(context, requirement, BaseAuthorizationHelper.ClaimGroups.AdminLevelsExcludingModerators);

            // Allow moderators to lock chat messages for their specific game type
            if (context.Resource is GameType gameType)
            {
                BaseAuthorizationHelper.CheckModeratorAccess(context, requirement, gameType);
            }
        }

        #endregion
    }
}

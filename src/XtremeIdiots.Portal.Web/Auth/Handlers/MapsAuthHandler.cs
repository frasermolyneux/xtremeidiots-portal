using Microsoft.AspNetCore.Authorization;
using XtremeIdiots.Portal.Repository.Abstractions.Constants.V1;
using XtremeIdiots.Portal.Web.Auth.Requirements;

namespace XtremeIdiots.Portal.Web.Auth.Handlers
{
    /// <summary>
    /// Handles authorization for map-related operations including access, management, and map pack operations.
    /// Supports senior admin, head admin, and game admin permission levels for game-specific map operations.
    /// </summary>
    public class MapsAuthHandler : IAuthorizationHandler
    {
        /// <summary>
        /// Handles authorization requirements for map operations.
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
                    case AccessMaps:
                        HandleAccessMaps(context, requirement);
                        break;
                    case AccessMapManagerController:
                        HandleAccessMapManagerController(context, requirement);
                        break;
                    case ManageMaps:
                        HandleManageMaps(context, requirement);
                        break;
                    case CreateMapPack:
                        HandleCreateMapPack(context, requirement);
                        break;
                    case EditMapPack:
                        HandleEditMapPack(context, requirement);
                        break;
                    case DeleteMapPack:
                        HandleDeleteMapPack(context, requirement);
                        break;
                    case PushMapToRemote:
                        HandlePushMapToRemote(context, requirement);
                        break;
                    case DeleteMapFromHost:
                        HandleDeleteMapFromHost(context, requirement);
                        break;
                }
            }

            return Task.CompletedTask;
        }

        #region Authorization Handlers

        /// <summary>
        /// Handles authorization for accessing maps.
        /// Allows senior admins, head admins, or game admins for the game type.
        /// </summary>
        /// <param name="context">The authorization context.</param>
        /// <param name="requirement">The access maps requirement.</param>
        private static void HandleAccessMaps(AuthorizationHandlerContext context, IAuthorizationRequirement requirement)
        {
            BaseAuthorizationHelper.CheckSeniorOrGameAdminAccessWithResource(context, requirement);
        }

        /// <summary>
        /// Handles authorization for accessing the map manager controller.
        /// Allows senior admins, head admins, or game admins for the game type.
        /// </summary>
        /// <param name="context">The authorization context.</param>
        /// <param name="requirement">The access map manager controller requirement.</param>
        private static void HandleAccessMapManagerController(AuthorizationHandlerContext context, IAuthorizationRequirement requirement)
        {
            BaseAuthorizationHelper.CheckSeniorOrGameAdminAccessWithResource(context, requirement);
        }

        /// <summary>
        /// Handles authorization for managing maps.
        /// Allows senior admins, head admins, or game admins for the game type.
        /// </summary>
        /// <param name="context">The authorization context.</param>
        /// <param name="requirement">The manage maps requirement.</param>
        private static void HandleManageMaps(AuthorizationHandlerContext context, IAuthorizationRequirement requirement)
        {
            BaseAuthorizationHelper.CheckSeniorOrGameAdminAccessWithResource(context, requirement);
        }

        /// <summary>
        /// Handles authorization for creating map packs.
        /// Allows senior admins, head admins, or game admins for the game type.
        /// </summary>
        /// <param name="context">The authorization context.</param>
        /// <param name="requirement">The create map pack requirement.</param>
        private static void HandleCreateMapPack(AuthorizationHandlerContext context, IAuthorizationRequirement requirement)
        {
            BaseAuthorizationHelper.CheckSeniorOrGameAdminAccessWithResource(context, requirement);
        }

        /// <summary>
        /// Handles authorization for editing map packs.
        /// Allows senior admins, head admins, or game admins for the game type.
        /// </summary>
        /// <param name="context">The authorization context.</param>
        /// <param name="requirement">The edit map pack requirement.</param>
        private static void HandleEditMapPack(AuthorizationHandlerContext context, IAuthorizationRequirement requirement)
        {
            BaseAuthorizationHelper.CheckSeniorOrGameAdminAccessWithResource(context, requirement);
        }

        /// <summary>
        /// Handles authorization for deleting map packs.
        /// Allows senior admins, head admins, or game admins for the game type.
        /// </summary>
        /// <param name="context">The authorization context.</param>
        /// <param name="requirement">The delete map pack requirement.</param>
        private static void HandleDeleteMapPack(AuthorizationHandlerContext context, IAuthorizationRequirement requirement)
        {
            BaseAuthorizationHelper.CheckSeniorOrGameAdminAccessWithResource(context, requirement);
        }

        /// <summary>
        /// Handles authorization for pushing maps to remote servers.
        /// Allows senior admins, head admins, or game admins for the game type.
        /// </summary>
        /// <param name="context">The authorization context.</param>
        /// <param name="requirement">The push map to remote requirement.</param>
        private static void HandlePushMapToRemote(AuthorizationHandlerContext context, IAuthorizationRequirement requirement)
        {
            BaseAuthorizationHelper.CheckSeniorOrGameAdminAccessWithResource(context, requirement);
        }

        /// <summary>
        /// Handles authorization for deleting maps from host servers.
        /// Allows senior admins, head admins, or game admins for the game type.
        /// </summary>
        /// <param name="context">The authorization context.</param>
        /// <param name="requirement">The delete map from host requirement.</param>
        private static void HandleDeleteMapFromHost(AuthorizationHandlerContext context, IAuthorizationRequirement requirement)
        {
            BaseAuthorizationHelper.CheckSeniorOrGameAdminAccessWithResource(context, requirement);
        }

        #endregion
    }
}

using Microsoft.AspNetCore.Authorization;
using XtremeIdiots.Portal.Repository.Abstractions.Constants.V1;
using XtremeIdiots.Portal.Web.Auth.Requirements;

namespace XtremeIdiots.Portal.Web.Auth.Handlers
{
    /// <summary>
    /// Handles authorization for player-related operations including access, deletion, protected names, and player tags.
    /// Supports different permission levels: senior admin only, admin levels (excluding moderators), and all admin levels (including moderators).
    /// </summary>
    public class PlayersAuthHandler : IAuthorizationHandler
    {
        /// <summary>
        /// Handles authorization requirements for player operations.
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
                    case AccessPlayers:
                        HandleAccessPlayers(context, requirement);
                        break;
                    case DeletePlayer:
                        HandleDeletePlayer(context, requirement);
                        break;
                    case ViewPlayers:
                        HandleViewPlayers(context, requirement);
                        break;
                    case CreateProtectedName:
                        HandleCreateProtectedName(context, requirement);
                        break;
                    case DeleteProtectedName:
                        HandleDeleteProtectedName(context, requirement);
                        break;
                    case ViewProtectedName:
                        HandleViewProtectedName(context, requirement);
                        break;
                    case CreatePlayerTag:
                        HandleCreatePlayerTag(context, requirement);
                        break;
                    case DeletePlayerTag:
                        HandleDeletePlayerTag(context, requirement);
                        break;
                    case ViewPlayerTag:
                        HandleViewPlayerTag(context, requirement);
                        break;
                }
            }

            return Task.CompletedTask;
        }

        #region Authorization Handlers

        /// <summary>
        /// Handles authorization for accessing players.
        /// Allows all admin levels including moderators.
        /// </summary>
        /// <param name="context">The authorization context.</param>
        /// <param name="requirement">The access players requirement.</param>
        private static void HandleAccessPlayers(AuthorizationHandlerContext context, IAuthorizationRequirement requirement)
        {
            BaseAuthorizationHelper.CheckClaimTypes(context, requirement, BaseAuthorizationHelper.ClaimGroups.AllAdminLevels);
        }

        /// <summary>
        /// Handles authorization for deleting players.
        /// Only allows senior admins.
        /// </summary>
        /// <param name="context">The authorization context.</param>
        /// <param name="requirement">The delete player requirement.</param>
        private static void HandleDeletePlayer(AuthorizationHandlerContext context, IAuthorizationRequirement requirement)
        {
            BaseAuthorizationHelper.CheckSeniorAdminAccess(context, requirement);
        }

        /// <summary>
        /// Handles authorization for viewing players.
        /// Allows all admin levels including moderators for the specific game type.
        /// </summary>
        /// <param name="context">The authorization context.</param>
        /// <param name="requirement">The view players requirement.</param>
        private static void HandleViewPlayers(AuthorizationHandlerContext context, IAuthorizationRequirement requirement)
        {
            BaseAuthorizationHelper.CheckSeniorOrMultipleGameAccessWithResource(context, requirement);
        }

        /// <summary>
        /// Handles authorization for creating protected names.
        /// Allows all admin levels including moderators.
        /// </summary>
        /// <param name="context">The authorization context.</param>
        /// <param name="requirement">The create protected name requirement.</param>
        private static void HandleCreateProtectedName(AuthorizationHandlerContext context, IAuthorizationRequirement requirement)
        {
            BaseAuthorizationHelper.CheckClaimTypes(context, requirement, BaseAuthorizationHelper.ClaimGroups.AllAdminLevels);
        }

        /// <summary>
        /// Handles authorization for deleting protected names.
        /// Allows all admin levels including moderators.
        /// </summary>
        /// <param name="context">The authorization context.</param>
        /// <param name="requirement">The delete protected name requirement.</param>
        private static void HandleDeleteProtectedName(AuthorizationHandlerContext context, IAuthorizationRequirement requirement)
        {
            BaseAuthorizationHelper.CheckClaimTypes(context, requirement, BaseAuthorizationHelper.ClaimGroups.AllAdminLevels);
        }

        /// <summary>
        /// Handles authorization for viewing protected names.
        /// Allows all admin levels including moderators.
        /// </summary>
        /// <param name="context">The authorization context.</param>
        /// <param name="requirement">The view protected name requirement.</param>
        private static void HandleViewProtectedName(AuthorizationHandlerContext context, IAuthorizationRequirement requirement)
        {
            BaseAuthorizationHelper.CheckClaimTypes(context, requirement, BaseAuthorizationHelper.ClaimGroups.AllAdminLevels);
        }

        /// <summary>
        /// Handles authorization for creating player tags.
        /// Allows admin levels excluding moderators.
        /// </summary>
        /// <param name="context">The authorization context.</param>
        /// <param name="requirement">The create player tag requirement.</param>
        private static void HandleCreatePlayerTag(AuthorizationHandlerContext context, IAuthorizationRequirement requirement)
        {
            BaseAuthorizationHelper.CheckClaimTypes(context, requirement, BaseAuthorizationHelper.ClaimGroups.AdminLevelsExcludingModerators);
        }

        /// <summary>
        /// Handles authorization for deleting player tags.
        /// Allows admin levels excluding moderators.
        /// </summary>
        /// <param name="context">The authorization context.</param>
        /// <param name="requirement">The delete player tag requirement.</param>
        private static void HandleDeletePlayerTag(AuthorizationHandlerContext context, IAuthorizationRequirement requirement)
        {
            BaseAuthorizationHelper.CheckClaimTypes(context, requirement, BaseAuthorizationHelper.ClaimGroups.AdminLevelsExcludingModerators);
        }

        /// <summary>
        /// Handles authorization for viewing player tags.
        /// Allows all admin levels including moderators.
        /// </summary>
        /// <param name="context">The authorization context.</param>
        /// <param name="requirement">The view player tag requirement.</param>
        private static void HandleViewPlayerTag(AuthorizationHandlerContext context, IAuthorizationRequirement requirement)
        {
            BaseAuthorizationHelper.CheckClaimTypes(context, requirement, BaseAuthorizationHelper.ClaimGroups.AllAdminLevels);
        }

        #endregion
    }
}

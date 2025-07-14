using Microsoft.AspNetCore.Authorization;
using XtremeIdiots.Portal.Repository.Abstractions.Constants.V1;
using XtremeIdiots.Portal.Web.Auth.Requirements;

namespace XtremeIdiots.Portal.Web.Auth.Handlers
{
    /// <summary>
    /// Handles authorization for player tag operations including access, creation, editing, and deletion.
    /// Supports different permission levels based on operation type and tag ownership.
    /// </summary>
    public class PlayerTagsAuthHandler : IAuthorizationHandler
    {
        /// <summary>
        /// Handles authorization requirements for player tag operations.
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
                    case AccessPlayerTags:
                        HandleAccessPlayerTags(context, requirement);
                        break;
                    case CreatePlayerTag:
                        HandleCreatePlayerTag(context, requirement);
                        break;
                    case EditPlayerTag:
                        HandleEditPlayerTag(context, requirement);
                        break;
                    case DeletePlayerTag:
                        HandleDeletePlayerTag(context, requirement);
                        break;
                }
            }

            return Task.CompletedTask;
        }

        #region Authorization Handlers

        /// <summary>
        /// Handles authorization for accessing player tags.
        /// Only allows senior admins.
        /// </summary>
        /// <param name="context">The authorization context.</param>
        /// <param name="requirement">The access player tags requirement.</param>
        private static void HandleAccessPlayerTags(AuthorizationHandlerContext context, IAuthorizationRequirement requirement)
        {
            BaseAuthorizationHelper.CheckSeniorAdminAccess(context, requirement);
        }

        /// <summary>
        /// Handles authorization for creating player tags.
        /// Allows admin levels excluding moderators (senior admins, head admins, and game admins).
        /// </summary>
        /// <param name="context">The authorization context.</param>
        /// <param name="requirement">The create player tag requirement.</param>
        private static void HandleCreatePlayerTag(AuthorizationHandlerContext context, IAuthorizationRequirement requirement)
        {
            BaseAuthorizationHelper.CheckClaimTypes(context, requirement, BaseAuthorizationHelper.ClaimGroups.AdminLevelsExcludingModerators);
        }

        /// <summary>
        /// Handles authorization for editing player tags.
        /// Only allows senior admins.
        /// </summary>
        /// <param name="context">The authorization context.</param>
        /// <param name="requirement">The edit player tag requirement.</param>
        private static void HandleEditPlayerTag(AuthorizationHandlerContext context, IAuthorizationRequirement requirement)
        {
            BaseAuthorizationHelper.CheckSeniorAdminAccess(context, requirement);
        }

        /// <summary>
        /// Handles authorization for deleting player tags.
        /// Senior admins can delete any tag. Head admins and game admins can delete UserDefined tags for their game type
        /// (actual filtering by game type and UserDefined will happen in the controller).
        /// </summary>
        /// <param name="context">The authorization context.</param>
        /// <param name="requirement">The delete player tag requirement.</param>
        private static void HandleDeletePlayerTag(AuthorizationHandlerContext context, IAuthorizationRequirement requirement)
        {
            BaseAuthorizationHelper.CheckClaimTypes(context, requirement, BaseAuthorizationHelper.ClaimGroups.AdminLevelsExcludingModerators);
        }

        #endregion
    }
}

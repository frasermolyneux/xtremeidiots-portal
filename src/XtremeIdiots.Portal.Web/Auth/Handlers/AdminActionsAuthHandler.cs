using Microsoft.AspNetCore.Authorization;

using XtremeIdiots.Portal.Web.Auth.Requirements;
using XtremeIdiots.Portal.Web.Extensions;
using XtremeIdiots.Portal.Repository.Abstractions.Constants.V1;

namespace XtremeIdiots.Portal.Web.Auth.Handlers
{
    /// <summary>
    /// Handles authorization requirements for admin actions based on user claims and game types.
    /// Evaluates permissions for creating, editing, deleting, and managing admin actions within the gaming portal.
    /// </summary>
    public class AdminActionsAuthHandler : IAuthorizationHandler
    {
        /// <summary>
        /// Evaluates authorization requirements for admin actions.
        /// </summary>
        /// <param name="context">The authorization context containing user claims and requirements.</param>
        /// <returns>A completed task representing the authorization evaluation.</returns>
        public Task HandleAsync(AuthorizationHandlerContext context)
        {
            var pendingRequirements = context.PendingRequirements.ToList();

            foreach (var requirement in pendingRequirements)
            {
                switch (requirement)
                {
                    case AccessAdminActions accessReq:
                        HandleAccessAdminActions(context, accessReq);
                        break;
                    case ChangeAdminActionAdmin changeReq:
                        HandleChangeAdminActionAdmin(context, changeReq);
                        break;
                    case ClaimAdminAction claimReq:
                        HandleClaimAdminAction(context, claimReq);
                        break;
                    case LiftAdminAction liftReq:
                        HandleLiftAdminAction(context, liftReq);
                        break;
                    case CreateAdminAction createReq:
                        HandleCreateAdminAction(context, createReq);
                        break;
                    case EditAdminAction editReq:
                        HandleEditAdminAction(context, editReq);
                        break;
                    case DeleteAdminAction deleteReq:
                        HandleDeleteAdminAction(context, deleteReq);
                        break;
                    case CreateAdminActionTopic topicReq:
                        HandleCreateAdminActionTopic(context, topicReq);
                        break;
                }
            }

            return Task.CompletedTask;
        }

        #region Helper Methods

        /// <summary>
        /// Checks if the user is the owner of the admin action (matches admin ID).
        /// </summary>
        /// <param name="context">The authorization context.</param>
        /// <param name="adminId">The admin ID to check ownership against.</param>
        /// <returns>True if the user is the owner, false otherwise.</returns>
        private static bool IsAdminActionOwner(AuthorizationHandlerContext context, string? adminId)
        {
            return BaseAuthorizationHelper.IsActionOwner(context, adminId);
        }

        #endregion

        #region Authorization Handlers

        /// <summary>
        /// Handles authorization for creating admin action topics.
        /// Requires senior admin, head admin, or game admin permissions for the specific game.
        /// </summary>
        /// <param name="context">The authorization context.</param>
        /// <param name="requirement">The create admin action topic requirement.</param>
        private static void HandleCreateAdminActionTopic(AuthorizationHandlerContext context, IAuthorizationRequirement requirement)
        {
            BaseAuthorizationHelper.CheckSeniorOrGameAdminAccessWithResource(context, requirement);
        }

        /// <summary>
        /// Handles authorization for deleting admin actions.
        /// Requires senior admin permissions only.
        /// </summary>
        /// <param name="context">The authorization context.</param>
        /// <param name="requirement">The delete admin action requirement.</param>
        private static void HandleDeleteAdminAction(AuthorizationHandlerContext context, IAuthorizationRequirement requirement)
        {
            BaseAuthorizationHelper.CheckSeniorAdminAccess(context, requirement);
        }        /// <summary>
                 /// Handles authorization for editing admin actions.
                 /// Permissions vary based on admin action type and user role.
                 /// </summary>
                 /// <param name="context">The authorization context.</param>
                 /// <param name="requirement">The edit admin action requirement.</param>
        private static void HandleEditAdminAction(AuthorizationHandlerContext context, IAuthorizationRequirement requirement)
        {
            BaseAuthorizationHelper.CheckSeniorAdminAccess(context, requirement);

            if (context.Resource is Tuple<GameType, AdminActionType, string?> resource)
            {
                var (gameType, adminActionType, adminId) = resource;

                BaseAuthorizationHelper.CheckHeadAdminAccess(context, requirement, gameType);
                CheckActionSpecificEditPermissions(context, requirement, gameType, adminActionType, adminId);
            }
        }

        /// <summary>
        /// Checks action-specific edit permissions based on admin action type.
        /// </summary>
        /// <param name="context">The authorization context.</param>
        /// <param name="requirement">The requirement to potentially succeed.</param>
        /// <param name="gameType">The game type.</param>
        /// <param name="adminActionType">The type of admin action.</param>
        /// <param name="adminId">The ID of the admin who created the action.</param>
        private static void CheckActionSpecificEditPermissions(AuthorizationHandlerContext context, IAuthorizationRequirement requirement, GameType gameType, AdminActionType adminActionType, string? adminId)
        {
            var gameTypeString = gameType.ToString();
            var isOwner = IsAdminActionOwner(context, adminId);
            var isModerator = context.User.HasClaim(UserProfileClaimType.Moderator, gameTypeString);
            var isGameAdmin = context.User.HasClaim(UserProfileClaimType.GameAdmin, gameTypeString);

            switch (adminActionType)
            {
                case AdminActionType.Observation:
                case AdminActionType.Warning:
                case AdminActionType.Kick:
                    if ((isModerator || isGameAdmin) && isOwner)
                    {
                        context.Succeed(requirement);
                    }
                    break;

                case AdminActionType.TempBan:
                case AdminActionType.Ban:
                    if (isGameAdmin && isOwner)
                    {
                        context.Succeed(requirement);
                    }
                    break;
            }
        }

        /// <summary>
        /// Handles authorization for creating admin actions.
        /// Permissions vary based on admin action type and user role.
        /// </summary>
        /// <param name="context">The authorization context.</param>
        /// <param name="requirement">The create admin action requirement.</param>
        private static void HandleCreateAdminAction(AuthorizationHandlerContext context, IAuthorizationRequirement requirement)
        {
            BaseAuthorizationHelper.CheckSeniorAdminAccess(context, requirement);

            if (context.Resource is Tuple<GameType, AdminActionType> resource)
            {
                var (gameType, adminActionType) = resource;

                BaseAuthorizationHelper.CheckGameAdminAccess(context, requirement, gameType);

                // Moderators can only create certain action types
                if (IsModeratorLevelAction(adminActionType))
                {
                    BaseAuthorizationHelper.CheckModeratorAccess(context, requirement, gameType);
                }
            }
        }

        /// <summary>
        /// Determines if the admin action type can be performed by moderators.
        /// </summary>
        /// <param name="adminActionType">The admin action type to check.</param>
        /// <returns>True if moderators can perform this action type, false otherwise.</returns>
        private static bool IsModeratorLevelAction(AdminActionType adminActionType)
        {
            return adminActionType == AdminActionType.Observation ||
                   adminActionType == AdminActionType.Warning ||
                   adminActionType == AdminActionType.Kick;
        }

        /// <summary>
        /// Handles authorization for lifting admin actions.
        /// Allows senior admins, head admins, or game admins who created the action.
        /// </summary>
        /// <param name="context">The authorization context.</param>
        /// <param name="requirement">The lift admin action requirement.</param>
        private static void HandleLiftAdminAction(AuthorizationHandlerContext context, IAuthorizationRequirement requirement)
        {
            BaseAuthorizationHelper.CheckSeniorAdminAccess(context, requirement);

            if (context.Resource is Tuple<GameType, string> resource)
            {
                var (gameType, adminId) = resource;

                BaseAuthorizationHelper.CheckHeadAdminAccess(context, requirement, gameType);

                // Game admins can only lift their own actions
                if (context.User.HasClaim(UserProfileClaimType.GameAdmin, gameType.ToString()) &&
                    IsAdminActionOwner(context, adminId))
                {
                    context.Succeed(requirement);
                }
            }
        }

        /// <summary>
        /// Handles authorization for claiming admin actions.
        /// Allows senior admins, head admins, or game admins for the specific game.
        /// </summary>
        /// <param name="context">The authorization context.</param>
        /// <param name="requirement">The claim admin action requirement.</param>
        private static void HandleClaimAdminAction(AuthorizationHandlerContext context, IAuthorizationRequirement requirement)
        {
            BaseAuthorizationHelper.CheckSeniorOrGameAdminAccessWithResource(context, requirement);
        }

        /// <summary>
        /// Handles authorization for changing admin action admin assignment.
        /// Requires senior admin or head admin permissions for the specific game.
        /// </summary>
        /// <param name="context">The authorization context.</param>
        /// <param name="requirement">The change admin action admin requirement.</param>
        private static void HandleChangeAdminActionAdmin(AuthorizationHandlerContext context, IAuthorizationRequirement requirement)
        {
            BaseAuthorizationHelper.CheckSeniorAdminAccess(context, requirement);

            if (context.Resource is GameType gameType)
            {
                BaseAuthorizationHelper.CheckHeadAdminAccess(context, requirement, gameType);
            }
        }

        /// <summary>
        /// Handles authorization for accessing admin actions.
        /// Allows any admin level (senior admin, head admin, game admin, or moderator).
        /// </summary>
        /// <param name="context">The authorization context.</param>
        /// <param name="requirement">The access admin actions requirement.</param>
        private static void HandleAccessAdminActions(AuthorizationHandlerContext context, IAuthorizationRequirement requirement)
        {
            BaseAuthorizationHelper.CheckClaimTypes(context, requirement, BaseAuthorizationHelper.ClaimGroups.AllAdminLevels);
        }

        #endregion
    }
}

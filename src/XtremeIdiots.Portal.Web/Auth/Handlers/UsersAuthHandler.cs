using Microsoft.AspNetCore.Authorization;
using XtremeIdiots.Portal.Repository.Abstractions.Constants.V1;
using XtremeIdiots.Portal.Web.Auth.Requirements;

namespace XtremeIdiots.Portal.Web.Auth.Handlers
{
    /// <summary>
    /// Handles authorization for user management operations including access, creating claims, and deleting claims.
    /// Supports senior admin and head admin permission levels with game-specific restrictions for claim management.
    /// </summary>
    public class UsersAuthHandler : IAuthorizationHandler
    {
        /// <summary>
        /// Handles authorization requirements for user management operations.
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
                    case AccessUsers:
                        HandleAccessUsers(context, requirement);
                        break;
                    case CreateUserClaim:
                        HandleCreateUserClaim(context, requirement);
                        break;
                    case DeleteUserClaim:
                        HandleDeleteUserClaim(context, requirement);
                        break;
                }
            }

            return Task.CompletedTask;
        }

        #region Authorization Handlers

        /// <summary>
        /// Handles authorization for accessing users.
        /// Allows senior admins or any head admin.
        /// </summary>
        /// <param name="context">The authorization context.</param>
        /// <param name="requirement">The access users requirement.</param>
        private static void HandleAccessUsers(AuthorizationHandlerContext context, IAuthorizationRequirement requirement)
        {
            BaseAuthorizationHelper.CheckClaimTypes(context, requirement, BaseAuthorizationHelper.ClaimGroups.SeniorAndHeadAdminOnly);
        }

        /// <summary>
        /// Handles authorization for creating user claims.
        /// Allows senior admins or head admins for the specific game type.
        /// </summary>
        /// <param name="context">The authorization context.</param>
        /// <param name="requirement">The create user claim requirement.</param>
        private static void HandleCreateUserClaim(AuthorizationHandlerContext context, IAuthorizationRequirement requirement)
        {
            BaseAuthorizationHelper.CheckSeniorAdminAccess(context, requirement);

            if (context.Resource is GameType gameType)
            {
                BaseAuthorizationHelper.CheckHeadAdminAccess(context, requirement, gameType);
            }
        }

        /// <summary>
        /// Handles authorization for deleting user claims.
        /// Allows senior admins or head admins for the specific game type.
        /// </summary>
        /// <param name="context">The authorization context.</param>
        /// <param name="requirement">The delete user claim requirement.</param>
        private static void HandleDeleteUserClaim(AuthorizationHandlerContext context, IAuthorizationRequirement requirement)
        {
            BaseAuthorizationHelper.CheckSeniorAdminAccess(context, requirement);

            if (context.Resource is GameType gameType)
            {
                BaseAuthorizationHelper.CheckHeadAdminAccess(context, requirement, gameType);
            }
        }

        #endregion
    }
}

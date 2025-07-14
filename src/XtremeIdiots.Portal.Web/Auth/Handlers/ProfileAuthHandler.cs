using Microsoft.AspNetCore.Authorization;
using XtremeIdiots.Portal.Web.Auth.Requirements;

namespace XtremeIdiots.Portal.Web.Auth.Handlers
{
    /// <summary>
    /// Handles authorization for profile access operations.
    /// Allows any authenticated user to access their own profile.
    /// </summary>
    public class ProfileAuthHandler : IAuthorizationHandler
    {
        /// <summary>
        /// Handles authorization requirements for profile operations.
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
                    case AccessProfile:
                        HandleAccessProfile(context, requirement);
                        break;
                }
            }

            return Task.CompletedTask;
        }

        #region Authorization Handlers

        /// <summary>
        /// Handles authorization for accessing user profiles.
        /// Allows access to any authenticated user for their own profile.
        /// </summary>
        /// <param name="context">The authorization context.</param>
        /// <param name="requirement">The access profile requirement.</param>
        private static void HandleAccessProfile(AuthorizationHandlerContext context, IAuthorizationRequirement requirement)
        {
            BaseAuthorizationHelper.CheckAuthenticated(context, requirement);
        }

        #endregion
    }
}

using Microsoft.AspNetCore.Authorization;
using XtremeIdiots.Portal.Web.Auth.Requirements;

namespace XtremeIdiots.Portal.Web.Auth.Handlers
{
    /// <summary>
    /// Handles authorization for home page access.
    /// Provides public access to the home page for all users.
    /// </summary>
    public class HomeAuthHandler : IAuthorizationHandler
    {
        /// <summary>
        /// Handles authorization requirements for home page operations.
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
                    case AccessHome:
                        HandleAccessHome(context, requirement);
                        break;
                }
            }

            return Task.CompletedTask;
        }

        #region Authorization Handlers

        /// <summary>
        /// Handles authorization for accessing the home page.
        /// Allows public access for all users.
        /// </summary>
        /// <param name="context">The authorization context.</param>
        /// <param name="requirement">The access home requirement.</param>
        private static void HandleAccessHome(AuthorizationHandlerContext context, IAuthorizationRequirement requirement)
        {
            // Home page has public access - always succeed
            context.Succeed(requirement);
        }

        #endregion
    }
}

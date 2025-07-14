using Microsoft.AspNetCore.Authorization;
using XtremeIdiots.Portal.Web.Auth.Requirements;

namespace XtremeIdiots.Portal.Web.Auth.Handlers
{
    /// <summary>
    /// Handles authorization for server access operations.
    /// Provides public access to server information for all users.
    /// </summary>
    public class ServersAuthHandler : IAuthorizationHandler
    {
        /// <summary>
        /// Handles authorization requirements for server operations.
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
                    case AccessServers:
                        HandleAccessServers(context, requirement);
                        break;
                }
            }

            return Task.CompletedTask;
        }

        #region Authorization Handlers

        /// <summary>
        /// Handles authorization for accessing servers.
        /// Allows public access for all users.
        /// </summary>
        /// <param name="context">The authorization context.</param>
        /// <param name="requirement">The access servers requirement.</param>
        private static void HandleAccessServers(AuthorizationHandlerContext context, IAuthorizationRequirement requirement)
        {
            // Server access has public access - always succeed
            context.Succeed(requirement);
        }

        #endregion
    }
}

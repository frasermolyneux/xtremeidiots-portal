using Microsoft.AspNetCore.Authorization;
using XtremeIdiots.Portal.Repository.Abstractions.Constants.V1;
using XtremeIdiots.Portal.Web.Auth.Requirements;

namespace XtremeIdiots.Portal.Web.Auth.Handlers
{
    /// <summary>
    /// Handles authorization for status access operations.
    /// Allows admin levels and ban file monitors to access system status information.
    /// </summary>
    public class StatusAuthHandler : IAuthorizationHandler
    {
        /// <summary>
        /// Handles authorization requirements for status operations.
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
                    case AccessStatus:
                        HandleAccessStatus(context, requirement);
                        break;
                }
            }

            return Task.CompletedTask;
        }

        #region Authorization Handlers

        /// <summary>
        /// Handles authorization for accessing system status.
        /// Allows senior admins, head admins, game admins, and ban file monitors.
        /// </summary>
        /// <param name="context">The authorization context.</param>
        /// <param name="requirement">The access status requirement.</param>
        private static void HandleAccessStatus(AuthorizationHandlerContext context, IAuthorizationRequirement requirement)
        {
            BaseAuthorizationHelper.CheckClaimTypes(context, requirement, BaseAuthorizationHelper.ClaimGroups.StatusAccessLevels);
        }

        #endregion
    }
}

using Microsoft.AspNetCore.Authorization;

using XtremeIdiots.Portal.Web.Auth.Requirements;

namespace XtremeIdiots.Portal.Web.Auth.Handlers
{
    /// <summary>
    /// Handles authorization requirements for accessing change logs.
    /// Currently allows public access to change log information.
    /// </summary>
    public class ChangeLogAuthHandler : IAuthorizationHandler
    {
        /// <summary>
        /// Evaluates authorization requirements for change log access.
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
                    case AccessChangeLog accessReq:
                        HandleAccessChangeLog(context, accessReq);
                        break;
                }
            }

            return Task.CompletedTask;
        }

        #region Authorization Handlers

        /// <summary>
        /// Handles authorization for accessing change logs.
        /// Currently allows public access - all users can view change logs.
        /// </summary>
        /// <param name="context">The authorization context.</param>
        /// <param name="requirement">The access change log requirement.</param>
        private static void HandleAccessChangeLog(AuthorizationHandlerContext context, IAuthorizationRequirement requirement)
        {
            // Change logs are publicly accessible - always succeed
            context.Succeed(requirement);
        }

        #endregion
    }
}

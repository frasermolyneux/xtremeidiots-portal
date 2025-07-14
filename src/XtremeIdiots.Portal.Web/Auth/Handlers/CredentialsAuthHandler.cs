using Microsoft.AspNetCore.Authorization;

using XtremeIdiots.Portal.Web.Auth.Requirements;
using XtremeIdiots.Portal.Repository.Abstractions.Constants.V1;

namespace XtremeIdiots.Portal.Web.Auth.Handlers
{
    /// <summary>
    /// Handles authorization requirements for accessing server credentials.
    /// Evaluates permissions for viewing RCON and FTP credentials based on user claims.
    /// </summary>
    public class CredentialsAuthHandler : IAuthorizationHandler
    {
        /// <summary>
        /// Evaluates authorization requirements for credentials access.
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
                    case AccessCredentials accessReq:
                        HandleAccessCredentials(context, accessReq);
                        break;
                }
            }

            return Task.CompletedTask;
        }

        #region Authorization Handlers

        /// <summary>
        /// Handles authorization for accessing server credentials.
        /// Allows senior admins, head admins, game admins, and users with specific credential permissions.
        /// </summary>
        /// <param name="context">The authorization context.</param>
        /// <param name="requirement">The access credentials requirement.</param>
        private static void HandleAccessCredentials(AuthorizationHandlerContext context, IAuthorizationRequirement requirement)
        {
            BaseAuthorizationHelper.CheckClaimTypes(context, requirement, BaseAuthorizationHelper.ClaimGroups.CredentialsAccessLevels);
        }

        #endregion
    }
}

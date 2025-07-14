using Microsoft.AspNetCore.Authorization;

using XtremeIdiots.Portal.Web.Auth.Requirements;
using XtremeIdiots.Portal.Repository.Abstractions.Constants.V1;

namespace XtremeIdiots.Portal.Web.Auth.Handlers
{
    /// <summary>
    /// Handles authorization requirements for ban file monitors based on user claims and game server access.
    /// Evaluates permissions for viewing, creating, editing, and deleting ban file monitors.
    /// </summary>
    public class BanFileMonitorsAuthHandler : IAuthorizationHandler
    {
        /// <summary>
        /// Evaluates authorization requirements for ban file monitor operations.
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
                    case ViewBanFileMonitor viewReq:
                        HandleViewBanFileMonitor(context, viewReq);
                        break;
                    case EditBanFileMonitor editReq:
                        HandleEditBanFileMonitor(context, editReq);
                        break;
                    case DeleteBanFileMonitor deleteReq:
                        HandleDeleteBanFileMonitor(context, deleteReq);
                        break;
                    case CreateBanFileMonitor createReq:
                        HandleCreateBanFileMonitor(context, createReq);
                        break;
                    case AccessBanFileMonitors accessReq:
                        HandleAccessBanFileMonitors(context, accessReq);
                        break;
                }
            }

            return Task.CompletedTask;
        }

        #region Authorization Handlers

        /// <summary>
        /// Handles authorization for accessing ban file monitors.
        /// Allows senior admins, head admins, or users with ban file monitor permissions.
        /// </summary>
        /// <param name="context">The authorization context.</param>
        /// <param name="requirement">The access ban file monitors requirement.</param>
        private static void HandleAccessBanFileMonitors(AuthorizationHandlerContext context, IAuthorizationRequirement requirement)
        {
            BaseAuthorizationHelper.CheckClaimTypes(context, requirement, BaseAuthorizationHelper.ClaimGroups.BanFileMonitorLevels);
        }

        /// <summary>
        /// Handles authorization for creating ban file monitors.
        /// Allows senior admins, head admins for the game type, or users with server-specific ban file monitor permissions.
        /// </summary>
        /// <param name="context">The authorization context.</param>
        /// <param name="requirement">The create ban file monitor requirement.</param>
        private static void HandleCreateBanFileMonitor(AuthorizationHandlerContext context, IAuthorizationRequirement requirement)
        {
            BaseAuthorizationHelper.CheckSeniorOrGameTypeServerAccessWithResource(context, requirement);
        }

        /// <summary>
        /// Handles authorization for deleting ban file monitors.
        /// Allows senior admins, head admins for the game type, or users with server-specific ban file monitor permissions.
        /// </summary>
        /// <param name="context">The authorization context.</param>
        /// <param name="requirement">The delete ban file monitor requirement.</param>
        private static void HandleDeleteBanFileMonitor(AuthorizationHandlerContext context, IAuthorizationRequirement requirement)
        {
            BaseAuthorizationHelper.CheckSeniorOrGameTypeServerAccessWithResource(context, requirement);
        }

        /// <summary>
        /// Handles authorization for editing ban file monitors.
        /// Allows senior admins, head admins for the game type, or users with server-specific ban file monitor permissions.
        /// </summary>
        /// <param name="context">The authorization context.</param>
        /// <param name="requirement">The edit ban file monitor requirement.</param>
        private static void HandleEditBanFileMonitor(AuthorizationHandlerContext context, IAuthorizationRequirement requirement)
        {
            BaseAuthorizationHelper.CheckSeniorOrGameTypeServerAccessWithResource(context, requirement);
        }

        /// <summary>
        /// Handles authorization for viewing ban file monitors.
        /// Allows senior admins, head admins for the game type, or users with server-specific ban file monitor permissions.
        /// </summary>
        /// <param name="context">The authorization context.</param>
        /// <param name="requirement">The view ban file monitor requirement.</param>
        private static void HandleViewBanFileMonitor(AuthorizationHandlerContext context, IAuthorizationRequirement requirement)
        {
            BaseAuthorizationHelper.CheckSeniorOrGameTypeServerAccessWithResource(context, requirement);
        }

        #endregion
    }
}

using Microsoft.AspNetCore.Authorization;

using XtremeIdiots.Portal.Web.Auth.Requirements;
using XtremeIdiots.Portal.Repository.Abstractions.Constants.V1;

namespace XtremeIdiots.Portal.Web.Auth.Handlers
{

    public class BanFileMonitorsAuthHandler : IAuthorizationHandler
    {

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

        private static void HandleAccessBanFileMonitors(AuthorizationHandlerContext context, IAuthorizationRequirement requirement)
        {
            BaseAuthorizationHelper.CheckClaimTypes(context, requirement, BaseAuthorizationHelper.ClaimGroups.BanFileMonitorLevels);
        }

        private static void HandleCreateBanFileMonitor(AuthorizationHandlerContext context, IAuthorizationRequirement requirement)
        {
            BaseAuthorizationHelper.CheckSeniorOrGameTypeServerAccessWithResource(context, requirement);
        }

        private static void HandleDeleteBanFileMonitor(AuthorizationHandlerContext context, IAuthorizationRequirement requirement)
        {
            BaseAuthorizationHelper.CheckSeniorOrGameTypeServerAccessWithResource(context, requirement);
        }

        private static void HandleEditBanFileMonitor(AuthorizationHandlerContext context, IAuthorizationRequirement requirement)
        {
            BaseAuthorizationHelper.CheckSeniorOrGameTypeServerAccessWithResource(context, requirement);
        }

        private static void HandleViewBanFileMonitor(AuthorizationHandlerContext context, IAuthorizationRequirement requirement)
        {
            BaseAuthorizationHelper.CheckSeniorOrGameTypeServerAccessWithResource(context, requirement);
        }

        #endregion
    }
}
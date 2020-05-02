using System;
using System.Security.Claims;
using XI.Portal.Auth.Contract.Constants;
using XI.Portal.Auth.Contract.Extensions;
using XI.Portal.Servers.Models;

namespace XI.Portal.Auth.RconMonitors.Extensions
{
    public static class RconMonitorFilterModelExtensions
    {
        public static RconMonitorFilterModel ApplyAuth(this RconMonitorFilterModel filterModel, ClaimsPrincipal claimsPrincipal)
        {
            var requiredClaims = new[] {XtremeIdiotsClaimTypes.SeniorAdmin, XtremeIdiotsClaimTypes.HeadAdmin, XtremeIdiotsClaimTypes.GameAdmin, PortalClaimTypes.RconMonitor};
            var (gameTypes, rconMonitorIds) = claimsPrincipal.ClaimedGamesAndItems(requiredClaims);

            filterModel.GameTypes = gameTypes;
            filterModel.RconMonitorIds = rconMonitorIds;

            return filterModel;
        }
    }
}
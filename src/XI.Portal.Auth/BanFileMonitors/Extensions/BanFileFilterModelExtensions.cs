using System;
using System.Security.Claims;
using XI.Portal.Auth.Contract.Constants;
using XI.Portal.Auth.Contract.Extensions;
using XI.Portal.Servers.Models;

namespace XI.Portal.Auth.BanFileMonitors.Extensions
{
    public static class BanFileMonitorModelFilterModelExtensions
    {
        public static BanFileMonitorFilterModel ApplyAuth(this BanFileMonitorFilterModel filterModel, ClaimsPrincipal claimsPrincipal)
        {
            var requiredClaims = new[] {XtremeIdiotsClaimTypes.SeniorAdmin, XtremeIdiotsClaimTypes.HeadAdmin, PortalClaimTypes.BanFileMonitor};
            var (gameTypes, banFileMonitorIds) = claimsPrincipal.ClaimedGamesAndItems(requiredClaims);

            filterModel.GameTypes = gameTypes;
            filterModel.BanFileMonitorIds = banFileMonitorIds;

            return filterModel;
        }
    }
}
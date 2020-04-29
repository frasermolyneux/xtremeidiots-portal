using System;
using System.Security.Claims;
using XI.Portal.Auth.Contract.Constants;
using XI.Portal.Auth.Contract.Extensions;
using XI.Portal.Servers.Models;

namespace XI.Portal.Auth.RconMonitors.Extensions
{
    public static class RconMonitorsFilterModelExtensions
    {
        public static GameServerFilterModel ApplyAuthForRconMonitors(this GameServerFilterModel filterModel, ClaimsPrincipal claimsPrincipal)
        {
            var requiredClaims = new[] {XtremeIdiotsClaimTypes.SeniorAdmin, XtremeIdiotsClaimTypes.HeadAdmin, PortalClaimTypes.RconMonitor};
            var (gameTypes, serverIds) = claimsPrincipal.ClaimedGamesAndItems(requiredClaims);

            filterModel.GameTypes = gameTypes;
            filterModel.ServerIds = serverIds;

            return filterModel;
        }
    }
}
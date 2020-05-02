using System;
using System.Security.Claims;
using XI.Portal.Auth.Contract.Constants;
using XI.Portal.Auth.Contract.Extensions;
using XI.Portal.Servers.Models;

namespace XI.Portal.Auth.FileMonitors.Extensions
{
    public static class FileMonitorFilterModelExtensions
    {
        public static FileMonitorFilterModel ApplyAuth(this FileMonitorFilterModel filterModel, ClaimsPrincipal claimsPrincipal)
        {
            var requiredClaims = new[] {XtremeIdiotsClaimTypes.SeniorAdmin, XtremeIdiotsClaimTypes.HeadAdmin, XtremeIdiotsClaimTypes.GameAdmin, PortalClaimTypes.FileMonitor};
            var (gameTypes, fileMonitorIds) = claimsPrincipal.ClaimedGamesAndItems(requiredClaims);

            filterModel.GameTypes = gameTypes;
            filterModel.FileMonitorIds = fileMonitorIds;

            return filterModel;
        }
    }
}
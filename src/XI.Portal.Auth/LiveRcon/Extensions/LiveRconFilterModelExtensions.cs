using System;
using System.Security.Claims;
using XI.Portal.Auth.Contract.Constants;
using XI.Portal.Auth.Contract.Extensions;
using XI.Portal.Servers.Models;

namespace XI.Portal.Auth.LiveRcon.Extensions
{
    public static class LiveRconFilterModelExtensions
    {
        public static GameServerFilterModel ApplyAuthForLiveRcon(this GameServerFilterModel filterModel, ClaimsPrincipal claimsPrincipal)
        {
            var requiredClaims = new[] {XtremeIdiotsClaimTypes.SeniorAdmin, XtremeIdiotsClaimTypes.HeadAdmin, XtremeIdiotsClaimTypes.GameAdmin, PortalClaimTypes.LiveRcon};
            var (gameTypes, serverIds) = claimsPrincipal.ClaimedGamesAndItems(requiredClaims);

            filterModel.GameTypes = gameTypes;
            filterModel.ServerIds = serverIds;

            return filterModel;
        }
    }
}
using System;
using System.Security.Claims;
using XI.Portal.Auth.Contract.Constants;
using XI.Portal.Auth.Contract.Extensions;
using XI.Portal.Servers.Models;

namespace XI.Portal.Auth.Credentials.Extensions
{
    public static class GameServerFilterModelExtensions
    {
        public static GameServerFilterModel ApplyAuthForCredentials(this GameServerFilterModel filterModel, ClaimsPrincipal claimsPrincipal)
        {
            var requiredClaims = new[] {XtremeIdiotsClaimTypes.SeniorAdmin, XtremeIdiotsClaimTypes.HeadAdmin, XtremeIdiotsClaimTypes.GameAdmin, PortalClaimTypes.FtpCredentials, PortalClaimTypes.RconCredentials};
            var (gameTypes, serverIds) = claimsPrincipal.ClaimedGamesAndItems(requiredClaims);

            filterModel.GameTypes = gameTypes;
            filterModel.ServerIds = serverIds;

            return filterModel;
        }
    }
}
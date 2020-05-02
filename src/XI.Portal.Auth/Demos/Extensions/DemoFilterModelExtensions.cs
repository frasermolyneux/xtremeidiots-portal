using System.Linq;
using System.Security.Claims;
using XI.CommonTypes;
using XI.Portal.Auth.Contract.Constants;
using XI.Portal.Auth.Contract.Extensions;
using XI.Portal.Demos.Models;

namespace XI.Portal.Auth.Demos.Extensions
{
    public static class DemoFilterModelExtensions
    {
        public static DemosFilterModel ApplyAuth(this DemosFilterModel filterModel, ClaimsPrincipal claimsPrincipal, GameType? gameType)
        {
            var requiredClaims = new[] {XtremeIdiotsClaimTypes.SeniorAdmin, XtremeIdiotsClaimTypes.HeadAdmin, XtremeIdiotsClaimTypes.GameAdmin};
            var gameTypes = claimsPrincipal.ClaimedGameTypes(requiredClaims);

            if (gameType != null)
            {
                // If the user has the required claims do not filter by user id
                filterModel.UserId = gameTypes.Contains((GameType) gameType) ? null : claimsPrincipal.XtremeIdiotsId();
            }
            else
            {
                filterModel.GameTypes = gameTypes;

                // If the user has any required claims for games do not filter by user id
                if (!gameTypes.Any()) filterModel.UserId = claimsPrincipal.XtremeIdiotsId();
            }

            return filterModel;
        }
    }
}
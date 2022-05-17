using System.Collections.Generic;
using XtremeIdiots.Portal.RepositoryApi.Abstractions.Constants;

namespace XI.Portal.Web.Constants
{
    public static class DemosSupportedGames
    {
        public static IEnumerable<GameType> Games
        {
            get
            {
                yield return GameType.CallOfDuty2;
                yield return GameType.CallOfDuty4;
                yield return GameType.CallOfDuty5;
            }
        }
    }
}
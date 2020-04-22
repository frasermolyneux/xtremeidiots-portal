using System.Collections.Generic;
using XI.CommonTypes;

namespace XI.Portal.Demos.Constants
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
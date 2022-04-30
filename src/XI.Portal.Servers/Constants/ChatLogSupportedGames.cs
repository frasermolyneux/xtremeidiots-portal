using System.Collections.Generic;

namespace XI.Portal.Servers.Constants
{
    public static class ChatLogSupportedGames
    {
        public static IEnumerable<string> Games
        {
            get
            {
                yield return "CallOfDuty2";
                yield return "CallOfDuty4";
                yield return "CallOfDuty5";
            }
        }
    }
}
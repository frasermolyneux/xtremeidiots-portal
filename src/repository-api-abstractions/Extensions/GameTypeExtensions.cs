using System;
using XtremeIdiots.Portal.RepositoryApi.Abstractions.Constants;

namespace XtremeIdiots.Portal.RepositoryApi.Abstractions.Extensions
{
    public static class GameTypeExtensions
    {
        public static GameType ToGameType(this string gameType)
        {
            return Enum.Parse<GameType>(gameType);
        }

        public static string DisplayName(this GameType gameType)
        {
            switch (gameType)
            {
                case GameType.CallOfDuty2:
                    return "Call of Duty 2";
                case GameType.CallOfDuty4:
                    return "Call of Duty 4";
                case GameType.CallOfDuty5:
                    return "Call of Duty 5";
            }

            return gameType.ToString();
        }

        public static string ShortDisplayName(this GameType gameType)
        {
            switch (gameType)
            {
                case GameType.CallOfDuty2:
                    return "COD2";
                case GameType.CallOfDuty4:
                    return "COD4";
                case GameType.CallOfDuty5:
                    return "COD5";
            }

            return gameType.ToString();
        }
    }
}

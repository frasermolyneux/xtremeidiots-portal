using XtremeIdiots.Portal.RepositoryApi.Abstractions.Constants;

namespace XtremeIdiots.Portal.RepositoryWebApi.Extensions
{
    public static class GameTypeExtensions
    {
        public static GameType ToGameType(this int gameType)
        {
            return (GameType)gameType;
        }

        public static int ToGameTypeInt(this GameType gameType)
        {
            return (int)gameType;
        }

        public static string DemoExtension(this GameType gameType)
        {
            switch (gameType)
            {
                case GameType.CallOfDuty2:
                    return "dm_1";
                case GameType.CallOfDuty4:
                    return "dm_1";
                case GameType.CallOfDuty5:
                    return "dm_6";
                default:
                    throw new Exception("Game Type not supported for demos");
            }
        }
    }
}
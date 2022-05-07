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
    }
}
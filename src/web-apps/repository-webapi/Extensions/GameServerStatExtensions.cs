using XtremeIdiots.Portal.DataLib;
using XtremeIdiots.Portal.RepositoryApi.Abstractions.Models.GameServers;

namespace XtremeIdiots.Portal.RepositoryWebApi.Extensions
{
    public static class GameServerStatExtensions
    {
        public static GameServerStatDto ToDto(this GameServerStat gameServerStat)
        {
            var gameServerStatDto = new GameServerStatDto
            {
                Id = gameServerStat.Id,
                PlayerCount = gameServerStat.PlayerCount,
                MapName = gameServerStat.MapName,
                Timestamp = gameServerStat.Timestamp
            };

            if (gameServerStat.GameServerId != null)
                gameServerStatDto.GameServerId = (Guid)gameServerStat.GameServerId;

            return gameServerStatDto;
        }
    }
}

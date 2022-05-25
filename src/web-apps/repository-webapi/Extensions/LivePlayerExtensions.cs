using XtremeIdiots.Portal.DataLib;
using XtremeIdiots.Portal.RepositoryApi.Abstractions.Models;

namespace XtremeIdiots.Portal.RepositoryWebApi.Extensions
{
    public static class LivePlayerExtensions
    {
        public static LivePlayerDto ToDto(this LivePlayer livePlayer)
        {
            var livePlayerDto = new LivePlayerDto
            {
                Id = livePlayer.Id,
                Name = livePlayer.Name,
                Score = livePlayer.Score,
                Ping = livePlayer.Ping,
                Num = livePlayer.Num,
                Rate = livePlayer.Rate,
                Team = livePlayer.Team,
                Time = livePlayer.Time,
                IpAddress = livePlayer.IpAddress,
                Lat = livePlayer.Lat,
                Long = livePlayer.Long,
                CountryCode = livePlayer.CountryCode,
                GameType = livePlayer.GameType.ToGameType(),
                GameServerServerId = livePlayer.GameServerServerId
            };

            return livePlayerDto;
        }
    }
}

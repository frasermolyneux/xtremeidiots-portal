using XtremeIdiots.Portal.DataLib;
using XtremeIdiots.Portal.RepositoryApi.Abstractions.Models;

namespace XtremeIdiots.Portal.RepositoryWebApi.Extensions
{
    public static class GameServerExtensions
    {
        public static GameServerDto ToDto(this GameServer gameServer)
        {
            var dto = new GameServerDto
            {
                Id = gameServer.ServerId,
                Title = gameServer.Title,
                GameType = gameServer.GameType.ToGameType(),
                Hostname = gameServer.Hostname,
                QueryPort = gameServer.QueryPort,
                FtpHostname = gameServer.FtpHostname,
                FtpUsername = gameServer.FtpUsername,
                FtpPassword = gameServer.FtpPassword,
                RconPassword = gameServer.RconPassword,
                ShowOnBannerServerList = gameServer.ShowOnBannerServerList,
                HtmlBanner = gameServer.HtmlBanner,
                BannerServerListPosition = gameServer.BannerServerListPosition,
                ShowOnPortalServerList = gameServer.ShowOnPortalServerList,
                ShowChatLog = gameServer.ShowChatLog
            };

            return dto;
        }
    }
}

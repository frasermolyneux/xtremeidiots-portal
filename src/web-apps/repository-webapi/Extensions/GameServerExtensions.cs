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
                HtmlBanner = gameServer.HtmlBanner,
                GameType = gameServer.GameType.ToGameType(),
                Hostname = gameServer.Hostname,
                QueryPort = gameServer.QueryPort,
                FtpHostname = gameServer.FtpHostname,
                FtpUsername = gameServer.FtpUsername,
                FtpPassword = gameServer.FtpPassword,
                LiveStatusEnabled = gameServer.LiveStatusEnabled,
                LiveTitle = gameServer.LiveTitle,
                LiveMap = gameServer.LiveMap,
                LiveMod = gameServer.LiveMod,
                LiveMaxPlayers = gameServer.LiveMaxPlayers,
                LiveCurrentPlayers = gameServer.LiveCurrentPlayers,
                LiveLastUpdated = gameServer.LiveLastUpdated,
                ShowOnBannerServerList = gameServer.ShowOnBannerServerList,
                BannerServerListPosition = gameServer.BannerServerListPosition,
                ShowOnPortalServerList = gameServer.ShowOnPortalServerList,
                ShowChatLog = gameServer.ShowChatLog,
                RconPassword = gameServer.RconPassword
            };

            return dto;
        }
    }
}

using XI.Portal.Data.Legacy.Models;
using XI.Portal.Servers.Dto;

namespace XI.Portal.Servers.Extensions
{
    public static class GameServerExtensions
    {
        public static GameServerDto ToDto(this GameServers gameServer)
        {
            var gameServerDto = new GameServerDto
            {
                ServerId = gameServer.ServerId,
                Title = gameServer.Title,
                GameType = gameServer.GameType,
                Hostname = gameServer.Hostname,
                QueryPort = gameServer.QueryPort,
                FtpHostname = gameServer.FtpHostname,
                FtpUsername = gameServer.FtpUsername,
                FtpPassword = gameServer.FtpPassword,
                RconPassword = gameServer.RconPassword,
                ShowOnBannerServerList = gameServer.ShowOnBannerServerList,
                BannerServerListPosition = gameServer.BannerServerListPosition,
                HtmlBanner = gameServer.HtmlBanner,
                ShowOnPortalServerList = gameServer.ShowOnPortalServerList,
                ShowChatLog = gameServer.ShowChatLog
            };


            return gameServerDto;
        }
    }
}
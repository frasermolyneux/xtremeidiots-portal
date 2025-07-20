using XtremeIdiots.Portal.Web.ViewModels;
using XtremeIdiots.Portal.Repository.Abstractions.Models.V1.GameServers;

namespace XtremeIdiots.Portal.Web.Extensions
{
    public static class GameServerDtoExtensions
    {
        public static GameServerViewModel ToViewModel(this GameServerDto gameServerDto)
        {
            var viewModel = new GameServerViewModel
            {
                GameServerId = gameServerDto.GameServerId,
                Title = gameServerDto.Title,
                GameType = gameServerDto.GameType,
                Hostname = gameServerDto.Hostname,
                QueryPort = gameServerDto.QueryPort,
                FtpHostname = gameServerDto.FtpHostname,
                FtpPort = gameServerDto.FtpPort,
                FtpUsername = gameServerDto.FtpUsername,
                FtpPassword = gameServerDto.FtpPassword,
                RconPassword = gameServerDto.RconPassword,
                LiveTrackingEnabled = gameServerDto.LiveTrackingEnabled,
                BannerServerListEnabled = gameServerDto.BannerServerListEnabled,
                ServerListPosition = gameServerDto.ServerListPosition,
                HtmlBanner = gameServerDto.HtmlBanner,
                PortalServerListEnabled = gameServerDto.PortalServerListEnabled,
                ChatLogEnabled = gameServerDto.ChatLogEnabled
            };

            return viewModel;
        }
    }
}
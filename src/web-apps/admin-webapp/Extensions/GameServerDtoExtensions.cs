using XtremeIdiots.Portal.AdminWebApp.ViewModels;
using XtremeIdiots.Portal.RepositoryApi.Abstractions.Models.GameServers;

namespace XtremeIdiots.Portal.AdminWebApp.Extensions
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
                LiveStatusEnabled = gameServerDto.LiveStatusEnabled,
                ShowOnBannerServerList = gameServerDto.ShowOnBannerServerList,
                BannerServerListPosition = gameServerDto.BannerServerListPosition,
                HtmlBanner = gameServerDto.HtmlBanner,
                ShowOnPortalServerList = gameServerDto.ShowOnPortalServerList,
                ShowChatLog = gameServerDto.ShowChatLog
            };

            return viewModel;
        }
    }
}

using XI.Portal.Web.Models;
using XtremeIdiots.Portal.RepositoryApi.Abstractions.Models;

namespace XI.Portal.Web.Extensions
{
    public static class GameServerDtoExtensions
    {
        public static GameServerViewModel ToViewModel(this GameServerDto gameServerDto)
        {
            var viewModel = new GameServerViewModel
            {
                ServerId = gameServerDto.Id,
                Title = gameServerDto.Title,
                GameType = gameServerDto.GameType,
                Hostname = gameServerDto.Hostname,
                QueryPort = gameServerDto.QueryPort,
                FtpHostname = gameServerDto.FtpHostname,
                FtpUsername = gameServerDto.FtpUsername,
                FtpPassword = gameServerDto.FtpPassword,
                RconPassword = gameServerDto.RconPassword,
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

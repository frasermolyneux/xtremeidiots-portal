using XtremeIdiots.Portal.RepositoryApi.Abstractions.Constants;

namespace XtremeIdiots.Portal.RepositoryApi.Abstractions.Models.GameServers
{
    public class CreateGameServerDto
    {
        public CreateGameServerDto(string title, GameType gameType, string hostname, int queryPort)
        {
            Title = title;
            GameType = gameType;
            Hostname = hostname;
            QueryPort = queryPort;
        }

        public string Title { get; set; }
        public GameType GameType { get; set; }
        public string Hostname { get; set; }
        public int QueryPort { get; set; }

        public string? FtpHostname { get; set; }
        public int FtpPort { get; set; }
        public string? FtpUsername { get; set; }
        public string? FtpPassword { get; set; }
        public string? RconPassword { get; set; }

        public bool LiveStatusEnabled { get; set; }
        public int BannerServerListPosition { get; set; }
        public bool ShowOnBannerServerList { get; set; }
        public bool ShowOnPortalServerList { get; set; }
        public bool ShowChatLog { get; set; }

        public string? HtmlBanner { get; set; }
    }
}

namespace XtremeIdiots.Portal.RepositoryApi.Abstractions.Models
{
    public class GameServerDto
    {
        public Guid Id { get; set; }
        public string Title { get; set; }
        public string GameType { get; set; }
        public string Hostname { get; set; }
        public int QueryPort { get; set; }
        public string FtpHostname { get; set; }
        public string FtpUsername { get; set; }
        public string FtpPassword { get; set; }
        public string RconPassword { get; set; }
        public bool ShowOnBannerServerList { get; set; }
        public string? HtmlBanner { get; set; }
        public int BannerServerListPosition { get; set; }
        public bool ShowOnPortalServerList { get; set; }
        public bool ShowChatLog { get; set; }
    }
}
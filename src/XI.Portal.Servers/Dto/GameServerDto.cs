using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using XI.CommonTypes;

namespace XI.Portal.Servers.Dto
{
    public class GameServerDto
    {
        public Guid ServerId { get; set; }
        [Required] [MaxLength(60)] public string Title { get; set; }
        [Required] [DisplayName("Game")] public GameType GameType { get; set; }
        public string Hostname { get; set; }

        [DisplayName("Query Port")] public int QueryPort { get; set; }

        [DisplayName("Ftp Hostname")] public string FtpHostname { get; set; }
        [DisplayName("Ftp Username")] public string FtpUsername { get; set; }
        [DisplayName("Ftp Password")] public string FtpPassword { get; set; }
        [DisplayName("Rcon Password")] public string RconPassword { get; set; }
        [DisplayName("Banner Server List")] public bool ShowOnBannerServerList { get; set; }

        [DisplayName("Position")] public int BannerServerListPosition { get; set; }

        [DataType(DataType.MultilineText)]
        [DisplayName("HTML Banner")]
        public string HtmlBanner { get; set; }

        [DisplayName("Portal Server List")] public bool ShowOnPortalServerList { get; set; }

        [DisplayName("Chat Log")] public bool ShowChatLog { get; set; }
    }
}
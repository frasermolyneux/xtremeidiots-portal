using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using XI.Portal.Data.Legacy.CommonTypes;

namespace XI.Portal.Data.Legacy.Models
{
    public class GameServers
    {
        public GameServers()
        {
            // ReSharper disable VirtualMemberCallInConstructor
            BanFileMonitors = new HashSet<BanFileMonitors>();
            ChatLogs = new HashSet<ChatLogs>();
            FileMonitors = new HashSet<FileMonitors>();
            LivePlayers = new HashSet<LivePlayers>();
            MapRotations = new HashSet<MapRotations>();
            RconMonitors = new HashSet<RconMonitors>();
            // ReSharper restore VirtualMemberCallInConstructor
        }

        public Guid ServerId { get; set; }

        [Required] [MaxLength(60)] public string Title { get; set; }

        [Required] [DisplayName("Game")] public GameType GameType { get; set; }

        public string Hostname { get; set; }

        [DisplayName("Query Port")] public int QueryPort { get; set; }

        [DisplayName("Ftp Hostname")] public string FtpHostname { get; set; }
        [DisplayName("Ftp Username")] public string FtpUsername { get; set; }
        [DisplayName("Ftp Password")] public string FtpPassword { get; set; }
        [DisplayName("Rcon Password")] public string RconPassword { get; set; }
        public string LiveTitle { get; set; }
        public string LiveMap { get; set; }
        public string LiveMod { get; set; }
        public int LiveMaxPlayers { get; set; }
        public int LiveCurrentPlayers { get; set; }
        public DateTime LiveLastUpdated { get; set; }

        [DisplayName("Banner Server List")] public bool ShowOnBannerServerList { get; set; }

        [DisplayName("Position")] public int BannerServerListPosition { get; set; }

        [DataType(DataType.MultilineText)]
        [DisplayName("HTML Banner")]
        public string HtmlBanner { get; set; }

        [DisplayName("Portal Server List")] public bool ShowOnPortalServerList { get; set; }

        [DisplayName("Chat Log")] public bool ShowChatLog { get; set; }

        public virtual ICollection<BanFileMonitors> BanFileMonitors { get; set; }
        public virtual ICollection<ChatLogs> ChatLogs { get; set; }
        public virtual ICollection<FileMonitors> FileMonitors { get; set; }
        public virtual ICollection<LivePlayers> LivePlayers { get; set; }
        public virtual ICollection<MapRotations> MapRotations { get; set; }
        public virtual ICollection<RconMonitors> RconMonitors { get; set; }
    }
}
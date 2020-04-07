using System;
using System.Collections.Generic;

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
        public string Title { get; set; }
        public int GameType { get; set; }
        public string Hostname { get; set; }
        public int QueryPort { get; set; }
        public string FtpHostname { get; set; }
        public string FtpUsername { get; set; }
        public string FtpPassword { get; set; }
        public string RconPassword { get; set; }
        public string LiveTitle { get; set; }
        public string LiveMap { get; set; }
        public string LiveMod { get; set; }
        public int LiveMaxPlayers { get; set; }
        public int LiveCurrentPlayers { get; set; }
        public DateTime LiveLastUpdated { get; set; }
        public bool ShowOnBannerServerList { get; set; }
        public int BannerServerListPosition { get; set; }
        public string HtmlBanner { get; set; }
        public bool ShowOnPortalServerList { get; set; }
        public bool ShowChatLog { get; set; }

        public virtual ICollection<BanFileMonitors> BanFileMonitors { get; set; }
        public virtual ICollection<ChatLogs> ChatLogs { get; set; }
        public virtual ICollection<FileMonitors> FileMonitors { get; set; }
        public virtual ICollection<LivePlayers> LivePlayers { get; set; }
        public virtual ICollection<MapRotations> MapRotations { get; set; }
        public virtual ICollection<RconMonitors> RconMonitors { get; set; }
    }
}
﻿// <auto-generated> This file has been auto generated by EF Core Power Tools. </auto-generated>
#nullable disable
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace XtremeIdiots.Portal.DataLib
{
    [Index("ServerId", Name = "IX_ServerId", IsUnique = true)]
    public partial class GameServer
    {
        public GameServer()
        {
            BanFileMonitors = new HashSet<BanFileMonitor>();
            ChatLogs = new HashSet<ChatLog>();
            GameServerEvents = new HashSet<GameServerEvent>();
            GameServerStats = new HashSet<GameServerStat>();
            LivePlayers = new HashSet<LivePlayer>();
        }

        [Key]
        public Guid ServerId { get; set; }
        [StringLength(60)]
        public string Title { get; set; }
        public string HtmlBanner { get; set; }
        public int GameType { get; set; }
        public string Hostname { get; set; }
        public int QueryPort { get; set; }
        public string FtpHostname { get; set; }
        public int FtpPort { get; set; }
        public string FtpUsername { get; set; }
        public string FtpPassword { get; set; }
        public bool LiveStatusEnabled { get; set; }
        public string LiveTitle { get; set; }
        public string LiveMap { get; set; }
        public string LiveMod { get; set; }
        public int LiveMaxPlayers { get; set; }
        public int LiveCurrentPlayers { get; set; }
        [Column(TypeName = "datetime")]
        public DateTime LiveLastUpdated { get; set; }
        public bool ShowOnBannerServerList { get; set; }
        public int BannerServerListPosition { get; set; }
        public bool ShowOnPortalServerList { get; set; }
        public bool ShowChatLog { get; set; }
        public string RconPassword { get; set; }

        [InverseProperty("GameServerServer")]
        public virtual ICollection<BanFileMonitor> BanFileMonitors { get; set; }
        [InverseProperty("GameServerServer")]
        public virtual ICollection<ChatLog> ChatLogs { get; set; }
        [InverseProperty("GameServer")]
        public virtual ICollection<GameServerEvent> GameServerEvents { get; set; }
        [InverseProperty("GameServer")]
        public virtual ICollection<GameServerStat> GameServerStats { get; set; }
        [InverseProperty("GameServerServer")]
        public virtual ICollection<LivePlayer> LivePlayers { get; set; }
    }
}
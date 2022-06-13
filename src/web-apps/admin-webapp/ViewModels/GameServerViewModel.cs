﻿using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

using XtremeIdiots.Portal.RepositoryApi.Abstractions.Constants;

namespace XtremeIdiots.Portal.AdminWebApp.ViewModels
{
    public class GameServerViewModel
    {
        public Guid GameServerId { get; set; }
        [Required][MaxLength(60)] public string? Title { get; set; }
        [Required][DisplayName("Game")] public GameType GameType { get; set; }
        [Required] public string? Hostname { get; set; }

        [Required][DisplayName("Query Port")] public int QueryPort { get; set; }

        [DisplayName("Ftp Hostname")] public string? FtpHostname { get; set; }
        [DisplayName("Ftp Port")] public int FtpPort { get; set; }
        [DisplayName("Ftp Username")] public string? FtpUsername { get; set; }
        [DisplayName("Ftp Password")] public string? FtpPassword { get; set; }
        [DisplayName("Rcon Password")] public string? RconPassword { get; set; }
        [DisplayName("Live Status Tracking")] public bool LiveStatusEnabled { get; set; }
        [DisplayName("Banner Server List")] public bool ShowOnBannerServerList { get; set; }

        [DisplayName("Position")] public int BannerServerListPosition { get; set; }

        [DataType(DataType.MultilineText)]
        [DisplayName("HTML Banner")]
        public string? HtmlBanner { get; set; }

        [DisplayName("Portal Server List")] public bool ShowOnPortalServerList { get; set; }

        [DisplayName("Chat Log")] public bool ShowChatLog { get; set; }
    }
}

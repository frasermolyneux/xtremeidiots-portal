﻿// <auto-generated> This file has been auto generated by EF Core Power Tools. </auto-generated>
#nullable disable
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace XtremeIdiots.Portal.DataLib
{
    [Index("GameType", Name = "IX_GameType")]
    [Index("GameType", "LastSeen", Name = "IX_GameTypeAndLastSeen")]
    [Index("PlayerId", Name = "IX_PlayerId", IsUnique = true)]
    public partial class Player
    {
        public Player()
        {
            AdminActions = new HashSet<AdminAction>();
            ChatLogs = new HashSet<ChatLog>();
            LivePlayers = new HashSet<LivePlayer>();
            MapVotes = new HashSet<MapVote>();
            PlayerAliases = new HashSet<PlayerAlias>();
            PlayerIpAddresses = new HashSet<PlayerIpAddress>();
            RecentPlayers = new HashSet<RecentPlayer>();
            Reports = new HashSet<Report>();
        }

        [Key]
        public Guid PlayerId { get; set; }
        public int GameType { get; set; }
        public string Username { get; set; }
        public string Guid { get; set; }
        [Column(TypeName = "datetime")]
        public DateTime FirstSeen { get; set; }
        [Column(TypeName = "datetime")]
        public DateTime LastSeen { get; set; }
        public string IpAddress { get; set; }

        [InverseProperty("PlayerPlayer")]
        public virtual ICollection<AdminAction> AdminActions { get; set; }
        [InverseProperty("PlayerPlayer")]
        public virtual ICollection<ChatLog> ChatLogs { get; set; }
        [InverseProperty("Player")]
        public virtual ICollection<LivePlayer> LivePlayers { get; set; }
        [InverseProperty("PlayerPlayer")]
        public virtual ICollection<MapVote> MapVotes { get; set; }
        [InverseProperty("PlayerPlayer")]
        public virtual ICollection<PlayerAlias> PlayerAliases { get; set; }
        [InverseProperty("PlayerPlayer")]
        public virtual ICollection<PlayerIpAddress> PlayerIpAddresses { get; set; }
        [InverseProperty("Player")]
        public virtual ICollection<RecentPlayer> RecentPlayers { get; set; }
        [InverseProperty("Player")]
        public virtual ICollection<Report> Reports { get; set; }
    }
}
﻿// <auto-generated> This file has been auto generated by EF Core Power Tools. </auto-generated>
#nullable disable
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace XtremeIdiots.Portal.DataLib
{
    [Index("GameServerId", Name = "IX_GameServers_GameServerId")]
    [Index("PlayerId", Name = "IX_Players_PlayerId")]
    public partial class RecentPlayer
    {
        [Key]
        public Guid RecentPlayerId { get; set; }
        public Guid? PlayerId { get; set; }
        public Guid? GameServerId { get; set; }
        [Required]
        [StringLength(60)]
        public string Name { get; set; }
        [StringLength(60)]
        public string IpAddress { get; set; }
        public double? Lat { get; set; }
        public double? Long { get; set; }
        [StringLength(60)]
        public string CountryCode { get; set; }
        public int GameType { get; set; }
        [Column(TypeName = "datetime")]
        public DateTime Timestamp { get; set; }

        [ForeignKey("GameServerId")]
        [InverseProperty("RecentPlayers")]
        public virtual GameServer GameServer { get; set; }
        [ForeignKey("PlayerId")]
        [InverseProperty("RecentPlayers")]
        public virtual Player Player { get; set; }
    }
}
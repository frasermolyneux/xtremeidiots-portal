﻿// <auto-generated> This file has been auto generated by EF Core Power Tools. </auto-generated>
#nullable disable
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace XtremeIdiots.Portal.DataLib
{
    [Index("GameServerServerId", Name = "IX_GameServer_ServerId")]
    public partial class LivePlayer
    {
        [Key]
        public Guid LivePlayerId { get; set; }
        public string Name { get; set; }
        public int Score { get; set; }
        public int Ping { get; set; }
        public string Team { get; set; }
        public TimeSpan Time { get; set; }
        [Column("GameServer_ServerId")]
        public Guid? GameServerServerId { get; set; }

        [ForeignKey("GameServerServerId")]
        [InverseProperty("LivePlayers")]
        public virtual GameServer GameServerServer { get; set; }
    }
}
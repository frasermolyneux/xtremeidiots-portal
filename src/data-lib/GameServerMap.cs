﻿// <auto-generated> This file has been auto generated by EF Core Power Tools. </auto-generated>
#nullable disable
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace XtremeIdiots.Portal.DataLib
{
    public partial class GameServerMap
    {
        [Key]
        public int Id { get; set; }
        public Guid? GameServerId { get; set; }
        public Guid? MapId { get; set; }
        [Column(TypeName = "datetime")]
        public DateTime Timestamp { get; set; }

        [ForeignKey("GameServerId")]
        [InverseProperty("GameServerMaps")]
        public virtual GameServer GameServer { get; set; }
        [ForeignKey("MapId")]
        [InverseProperty("GameServerMaps")]
        public virtual Map Map { get; set; }
    }
}
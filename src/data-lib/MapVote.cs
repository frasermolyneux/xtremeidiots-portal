﻿// <auto-generated> This file has been auto generated by EF Core Power Tools. </auto-generated>
#nullable disable
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace XtremeIdiots.Portal.DataLib
{
    [Index("MapId", Name = "IX_Maps_MapId")]
    [Index("PlayerId", Name = "IX_Players_PlayerId")]
    public partial class MapVote
    {
        [Key]
        public Guid MapVoteId { get; set; }
        public Guid? MapId { get; set; }
        public Guid? PlayerId { get; set; }
        public bool Like { get; set; }
        [Column(TypeName = "datetime")]
        public DateTime Timestamp { get; set; }

        [ForeignKey("MapId")]
        [InverseProperty("MapVotes")]
        public virtual Map Map { get; set; }
        [ForeignKey("PlayerId")]
        [InverseProperty("MapVotes")]
        public virtual Player Player { get; set; }
    }
}
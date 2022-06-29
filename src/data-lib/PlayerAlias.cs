﻿// <auto-generated> This file has been auto generated by EF Core Power Tools. </auto-generated>
#nullable disable
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace XtremeIdiots.Portal.DataLib
{
    [Table("PlayerAlias")]
    [Index("Name", Name = "IX_Name")]
    [Index("PlayerId", Name = "IX_PlayerId")]
    public partial class PlayerAlias
    {
        [Key]
        public Guid PlayerAliasId { get; set; }
        public Guid? PlayerId { get; set; }
        [StringLength(60)]
        public string Name { get; set; }
        [Column(TypeName = "datetime")]
        public DateTime Added { get; set; }
        [Column(TypeName = "datetime")]
        public DateTime LastUsed { get; set; }

        [ForeignKey("PlayerId")]
        [InverseProperty("PlayerAliases")]
        public virtual Player Player { get; set; }
    }
}
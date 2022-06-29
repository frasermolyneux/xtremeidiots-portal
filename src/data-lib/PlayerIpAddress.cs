﻿// <auto-generated> This file has been auto generated by EF Core Power Tools. </auto-generated>
#nullable disable
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace XtremeIdiots.Portal.DataLib
{
    [Index("Address", Name = "IX_Address")]
    [Index("PlayerId", Name = "IX_Players_PlayerId")]
    public partial class PlayerIpAddress
    {
        [Key]
        public Guid PlayerIpAddressId { get; set; }
        public Guid? PlayerId { get; set; }
        [StringLength(60)]
        public string Address { get; set; }
        [Column(TypeName = "datetime")]
        public DateTime Added { get; set; }
        [Column(TypeName = "datetime")]
        public DateTime LastUsed { get; set; }

        [ForeignKey("PlayerId")]
        [InverseProperty("PlayerIpAddresses")]
        public virtual Player Player { get; set; }
    }
}
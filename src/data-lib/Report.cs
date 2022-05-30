﻿// <auto-generated> This file has been auto generated by EF Core Power Tools. </auto-generated>
#nullable disable
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace XtremeIdiots.Portal.DataLib
{
    [Index("ServerId", Name = "IX_GameServer_ServerId")]
    [Index("PlayerId", Name = "IX_Players_PlayerId")]
    [Index("UserProfileId", Name = "IX_UserProfiles_Id")]
    public partial class Report
    {
        [Key]
        public Guid Id { get; set; }
        public Guid? PlayerId { get; set; }
        public Guid? UserProfileId { get; set; }
        public Guid? ServerId { get; set; }
        public int GameType { get; set; }
        public string Comments { get; set; }
        [Column(TypeName = "datetime")]
        public DateTime Timestamp { get; set; }
        public Guid? AdminUserProfileId { get; set; }
        public string AdminClosingComments { get; set; }
        public bool Closed { get; set; }
        [Column(TypeName = "datetime")]
        public DateTime? ClosedTimestamp { get; set; }

        [ForeignKey("AdminUserProfileId")]
        [InverseProperty("ReportAdminUserProfiles")]
        public virtual UserProfile AdminUserProfile { get; set; }
        [ForeignKey("PlayerId")]
        [InverseProperty("Reports")]
        public virtual Player Player { get; set; }
        [ForeignKey("ServerId")]
        [InverseProperty("Reports")]
        public virtual GameServer Server { get; set; }
        [ForeignKey("UserProfileId")]
        [InverseProperty("ReportUserProfiles")]
        public virtual UserProfile UserProfile { get; set; }
    }
}
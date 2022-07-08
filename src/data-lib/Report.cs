﻿// <auto-generated> This file has been auto generated by EF Core Power Tools. </auto-generated>
#nullable disable
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace XtremeIdiots.Portal.DataLib
{
    [Index("AdminUserProfileId", Name = "IX_AdminUserProfileId")]
    [Index("GameServerId", Name = "IX_GameServerId")]
    [Index("PlayerId", Name = "IX_PlayerId")]
    [Index("ReportId", Name = "IX_ReportId", IsUnique = true)]
    [Index("UserProfileId", Name = "IX_UserProfileId")]
    public partial class Report
    {
        [Key]
        public Guid ReportId { get; set; }
        public Guid? PlayerId { get; set; }
        public Guid? UserProfileId { get; set; }
        public Guid? GameServerId { get; set; }
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
        [ForeignKey("GameServerId")]
        [InverseProperty("Reports")]
        public virtual GameServer GameServer { get; set; }
        [ForeignKey("PlayerId")]
        [InverseProperty("Reports")]
        public virtual Player Player { get; set; }
        [ForeignKey("UserProfileId")]
        [InverseProperty("ReportUserProfiles")]
        public virtual UserProfile UserProfile { get; set; }
    }
}
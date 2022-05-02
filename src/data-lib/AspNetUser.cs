﻿// <auto-generated> This file has been auto generated by EF Core Power Tools. </auto-generated>
#nullable disable
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace XtremeIdiots.Portal.DataLib
{
    [Index("UserName", Name = "UserNameIndex", IsUnique = true)]
    public partial class AspNetUser
    {
        public AspNetUser()
        {
            AdminActions = new HashSet<AdminAction>();
            AspNetUserClaims = new HashSet<AspNetUserClaim>();
            AspNetUserLogins = new HashSet<AspNetUserLogin>();
            Demos = new HashSet<Demo>();
            UserLogs = new HashSet<UserLog>();
            Roles = new HashSet<AspNetRole>();
        }

        [Key]
        [StringLength(128)]
        public string Id { get; set; }
        public string XtremeIdiotsId { get; set; }
        public string XtremeIdiotsTitle { get; set; }
        public string XtremeIdiotsFormattedName { get; set; }
        public string XtremeIdiotsPrimaryGroupId { get; set; }
        public string XtremeIdiotsPrimaryGroupName { get; set; }
        public string XtremeIdiotsPrimaryGroupIdFormattedName { get; set; }
        public string XtremeIdiotsPhotoUrl { get; set; }
        public string XtremeIdiotsPhotoUrlIsDefault { get; set; }
        [StringLength(256)]
        public string Email { get; set; }
        public bool EmailConfirmed { get; set; }
        public string PasswordHash { get; set; }
        public string SecurityStamp { get; set; }
        public string PhoneNumber { get; set; }
        public bool PhoneNumberConfirmed { get; set; }
        public bool TwoFactorEnabled { get; set; }
        [Column(TypeName = "datetime")]
        public DateTime? LockoutEndDateUtc { get; set; }
        public bool LockoutEnabled { get; set; }
        public int AccessFailedCount { get; set; }
        [Required]
        [StringLength(256)]
        public string UserName { get; set; }
        public string DemoManagerAuthKey { get; set; }

        [InverseProperty("Admin")]
        public virtual ICollection<AdminAction> AdminActions { get; set; }
        [InverseProperty("User")]
        public virtual ICollection<AspNetUserClaim> AspNetUserClaims { get; set; }
        [InverseProperty("User")]
        public virtual ICollection<AspNetUserLogin> AspNetUserLogins { get; set; }
        [InverseProperty("User")]
        public virtual ICollection<Demo> Demos { get; set; }
        [InverseProperty("ApplicationUser")]
        public virtual ICollection<UserLog> UserLogs { get; set; }

        [ForeignKey("UserId")]
        [InverseProperty("Users")]
        public virtual ICollection<AspNetRole> Roles { get; set; }
    }
}
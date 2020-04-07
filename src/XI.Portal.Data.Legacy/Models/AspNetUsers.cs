using System;
using System.Collections.Generic;

namespace XI.Portal.Data.Legacy.Models
{
    public class AspNetUsers
    {
        public AspNetUsers()
        {
            // ReSharper disable VirtualMemberCallInConstructor
            AdminActions = new HashSet<AdminActions>();
            AspNetUserClaims = new HashSet<AspNetUserClaims>();
            AspNetUserLogins = new HashSet<AspNetUserLogins>();
            AspNetUserRoles = new HashSet<AspNetUserRoles>();
            Demoes = new HashSet<Demoes>();
            UserLogs = new HashSet<UserLogs>();
            // ReSharper restore VirtualMemberCallInConstructor
        }

        public string Id { get; set; }
        public string XtremeIdiotsId { get; set; }
        public string XtremeIdiotsTitle { get; set; }
        public string XtremeIdiotsFormattedName { get; set; }
        public string XtremeIdiotsPrimaryGroupId { get; set; }
        public string XtremeIdiotsPrimaryGroupName { get; set; }
        public string XtremeIdiotsPrimaryGroupIdFormattedName { get; set; }
        public string XtremeIdiotsPhotoUrl { get; set; }
        public string XtremeIdiotsPhotoUrlIsDefault { get; set; }
        public string DemoManagerAuthKey { get; set; }
        public string Email { get; set; }
        public bool EmailConfirmed { get; set; }
        public string PasswordHash { get; set; }
        public string SecurityStamp { get; set; }
        public string PhoneNumber { get; set; }
        public bool PhoneNumberConfirmed { get; set; }
        public bool TwoFactorEnabled { get; set; }
        public DateTime? LockoutEndDateUtc { get; set; }
        public bool LockoutEnabled { get; set; }
        public int AccessFailedCount { get; set; }
        public string UserName { get; set; }

        public virtual ICollection<AdminActions> AdminActions { get; set; }
        public virtual ICollection<AspNetUserClaims> AspNetUserClaims { get; set; }
        public virtual ICollection<AspNetUserLogins> AspNetUserLogins { get; set; }
        public virtual ICollection<AspNetUserRoles> AspNetUserRoles { get; set; }
        public virtual ICollection<Demoes> Demoes { get; set; }
        public virtual ICollection<UserLogs> UserLogs { get; set; }
    }
}
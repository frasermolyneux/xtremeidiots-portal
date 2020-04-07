using System.Collections.Generic;

namespace XI.Portal.Data.Legacy.Models
{
    public class AspNetRoles
    {
        public AspNetRoles()
        {
            // ReSharper disable once VirtualMemberCallInConstructor
            AspNetUserRoles = new HashSet<AspNetUserRoles>();
        }

        public string Id { get; set; }
        public string Name { get; set; }

        public virtual ICollection<AspNetUserRoles> AspNetUserRoles { get; set; }
    }
}
using System.Collections.Generic;

namespace XI.Portal.Web.Models
{
    public class UserListEntryViewModel
    {
        public string Id { get; set; }
        public string Username { get; set; }
        public string Email { get; set; }

        public List<IPortalClaimDto> PortalClaims { get; set; }
    }
}
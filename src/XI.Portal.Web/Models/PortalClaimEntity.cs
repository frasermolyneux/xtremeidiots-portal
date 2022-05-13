using Microsoft.Azure.Cosmos.Table;

namespace XI.Portal.Users.Models
{
    internal class PortalClaimEntity : TableEntity, IPortalClaimDto
    {
        public string ClaimType { get; set; }
        public string ClaimValue { get; set; }
    }
}
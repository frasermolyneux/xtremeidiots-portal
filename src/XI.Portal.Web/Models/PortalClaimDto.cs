namespace XI.Portal.Web.Models
{
    public class PortalClaimDto : IPortalClaimDto
    {
        public string RowKey { get; set; }
        public string ClaimType { get; set; }
        public string ClaimValue { get; set; }
    }
}
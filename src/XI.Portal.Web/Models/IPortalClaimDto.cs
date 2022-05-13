namespace XI.Portal.Web.Models
{
    public interface IPortalClaimDto
    {
        string RowKey { get; set; }
        string ClaimType { get; set; }
        string ClaimValue { get; set; }
    }
}
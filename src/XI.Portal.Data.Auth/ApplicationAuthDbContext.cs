using ElCamino.AspNetCore.Identity.AzureTable;
using ElCamino.AspNetCore.Identity.AzureTable.Model;

namespace XI.Portal.Data.Auth
{
    public class ApplicationAuthDbContext : IdentityCloudContext
    {
        public ApplicationAuthDbContext()
        {
        }

        public ApplicationAuthDbContext(IdentityConfiguration config) : base(config)
        {
        }
    }
}
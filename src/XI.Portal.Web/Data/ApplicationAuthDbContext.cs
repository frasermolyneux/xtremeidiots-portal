﻿using ElCamino.AspNetCore.Identity.AzureTable;
using ElCamino.AspNetCore.Identity.AzureTable.Model;

namespace XI.Portal.Users.Data
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
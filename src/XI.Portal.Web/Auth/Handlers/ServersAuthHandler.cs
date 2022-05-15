﻿using Microsoft.AspNetCore.Authorization;
using System.Linq;
using System.Threading.Tasks;
using XI.Portal.Web.Auth.Requirements;

namespace XI.Portal.Web.Auth.Handlers
{
    public class ServersAuthHandler : IAuthorizationHandler
    {
        public Task HandleAsync(AuthorizationHandlerContext context)
        {
            var pendingRequirements = context.PendingRequirements.ToList();

            foreach (var requirement in pendingRequirements)
            {
                if (requirement is AccessServers)
                    HandleAccessServers(context, requirement);
            }

            return Task.CompletedTask;
        }

        private void HandleAccessServers(AuthorizationHandlerContext context, IAuthorizationRequirement requirement)
        {
            context.Succeed(requirement);
        }
    }
}
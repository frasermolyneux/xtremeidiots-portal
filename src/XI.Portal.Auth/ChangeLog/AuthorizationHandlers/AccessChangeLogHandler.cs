using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using XI.Portal.Auth.ChangeLog.AuthorizationRequirements;

namespace XI.Portal.Auth.ChangeLog.AuthorizationHandlers
{
    public class AccessChangeLogHandler : AuthorizationHandler<AccessChangeLog>
    {
        protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, AccessChangeLog requirement)
        {
            context.Succeed(requirement);
            return Task.CompletedTask;
        }
    }
}
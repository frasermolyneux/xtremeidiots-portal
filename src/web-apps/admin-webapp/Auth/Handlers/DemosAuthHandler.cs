using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using System;
using System.Linq;
using System.Threading.Tasks;
using XtremeIdiots.Portal.AdminWebApp.Auth.Constants;
using XtremeIdiots.Portal.AdminWebApp.Auth.Requirements;
using XtremeIdiots.Portal.AdminWebApp.Extensions;
using XtremeIdiots.Portal.RepositoryApi.Abstractions.Constants;

namespace XtremeIdiots.Portal.AdminWebApp.Auth.Handlers
{
    public class DemosAuthHandler : IAuthorizationHandler
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public DemosAuthHandler(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public Task HandleAsync(AuthorizationHandlerContext context)
        {
            var pendingRequirements = context.PendingRequirements.ToList();

            foreach (var requirement in pendingRequirements)
            {
                if (requirement is AccessDemos)
                    HandleAccessDemos(context, requirement);

                if (requirement is DeleteDemo)
                    HandleDeleteDemo(context, requirement);
            }

            return Task.CompletedTask;
        }

        private void HandleDeleteDemo(AuthorizationHandlerContext context, IAuthorizationRequirement requirement)
        {
            if (context.User.HasClaim(claim => claim.Type == XtremeIdiotsClaimTypes.SeniorAdmin))
                context.Succeed(requirement);

            if (context.Resource is Tuple<GameType, string>)
            {
                var (gameType, userId) = (Tuple<GameType, string>)context.Resource;

                if (context.User.HasClaim(XtremeIdiotsClaimTypes.HeadAdmin, gameType.ToString()))
                    context.Succeed(requirement);

                if (context.User.XtremeIdiotsId() == userId)
                    context.Succeed(requirement);
            }
        }

        private void HandleAccessDemos(AuthorizationHandlerContext context, IAuthorizationRequirement requirement)
        {
            if (context.User.Identity.IsAuthenticated)
                context.Succeed(requirement);

            var httpContext = _httpContextAccessor.HttpContext;
            if (httpContext.Request.Headers.ContainsKey("demo-manager-auth-key"))
                context.Succeed(requirement);
        }
    }
}

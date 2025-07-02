using Microsoft.AspNetCore.Authorization;

using XtremeIdiots.Portal.Web.Auth.Requirements;
using XtremeIdiots.Portal.Web.Extensions;
using XtremeIdiots.Portal.RepositoryApi.Abstractions.Constants;

namespace XtremeIdiots.Portal.Web.Auth.Handlers
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
            if (context.User.HasClaim(claim => claim.Type == UserProfileClaimType.SeniorAdmin))
                context.Succeed(requirement);

            if (context.Resource is Tuple<GameType, Guid>)
            {
                var (gameType, userProfileId) = (Tuple<GameType, Guid>)context.Resource;

                if (context.User.HasClaim(UserProfileClaimType.HeadAdmin, gameType.ToString()))
                    context.Succeed(requirement);

                if (context.User.UserProfileId() == userProfileId.ToString())
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

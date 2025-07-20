using Microsoft.AspNetCore.Authorization;

using XtremeIdiots.Portal.Web.Auth.Requirements;
using XtremeIdiots.Portal.Web.Extensions;
using XtremeIdiots.Portal.Repository.Abstractions.Constants.V1;

namespace XtremeIdiots.Portal.Web.Auth.Handlers
{

    public class DemosAuthHandler : IAuthorizationHandler
    {
        private readonly IHttpContextAccessor httpContextAccessor;

        public DemosAuthHandler(IHttpContextAccessor httpContextAccessor)
        {
            this.httpContextAccessor = httpContextAccessor ?? throw new ArgumentNullException(nameof(httpContextAccessor));
        }

        public Task HandleAsync(AuthorizationHandlerContext context)
        {
            var pendingRequirements = context.PendingRequirements.ToList();

            foreach (var requirement in pendingRequirements)
            {
                switch (requirement)
                {
                    case AccessDemos accessReq:
                        HandleAccessDemos(context, accessReq);
                        break;
                    case DeleteDemo deleteReq:
                        HandleDeleteDemo(context, deleteReq);
                        break;
                }
            }

            return Task.CompletedTask;
        }

        private void HandleDeleteDemo(AuthorizationHandlerContext context, IAuthorizationRequirement requirement)
        {
            BaseAuthorizationHelper.CheckSeniorAdminAccess(context, requirement);

            if (context.Resource is Tuple<GameType, Guid> resource)
            {
                var (gameType, userProfileId) = resource;

                BaseAuthorizationHelper.CheckHeadAdminAccess(context, requirement, gameType);

                if (BaseAuthorizationHelper.IsResourceOwner(context, userProfileId))
                {
                    context.Succeed(requirement);
                }
            }
        }

        private void HandleAccessDemos(AuthorizationHandlerContext context, IAuthorizationRequirement requirement)
        {

            BaseAuthorizationHelper.CheckAuthenticated(context, requirement);

            var httpContext = httpContextAccessor.HttpContext;
            if (httpContext?.Request.Headers.ContainsKey("demo-manager-auth-key") == true)
            {
                context.Succeed(requirement);
            }
        }
    }
}
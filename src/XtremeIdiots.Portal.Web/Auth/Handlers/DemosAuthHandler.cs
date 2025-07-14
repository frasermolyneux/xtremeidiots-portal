using Microsoft.AspNetCore.Authorization;

using XtremeIdiots.Portal.Web.Auth.Requirements;
using XtremeIdiots.Portal.Web.Extensions;
using XtremeIdiots.Portal.Repository.Abstractions.Constants.V1;

namespace XtremeIdiots.Portal.Web.Auth.Handlers
{
    /// <summary>
    /// Handles authorization requirements for accessing and managing demos.
    /// Supports both authenticated user access and demo manager API access via headers.
    /// </summary>
    public class DemosAuthHandler : IAuthorizationHandler
    {
        private readonly IHttpContextAccessor httpContextAccessor;

        /// <summary>
        /// Initializes a new instance of the DemosAuthHandler class.
        /// </summary>
        /// <param name="httpContextAccessor">HTTP context accessor for checking request headers.</param>
        public DemosAuthHandler(IHttpContextAccessor httpContextAccessor)
        {
            this.httpContextAccessor = httpContextAccessor ?? throw new ArgumentNullException(nameof(httpContextAccessor));
        }

        /// <summary>
        /// Evaluates authorization requirements for demo operations.
        /// </summary>
        /// <param name="context">The authorization context containing user claims and requirements.</param>
        /// <returns>A completed task representing the authorization evaluation.</returns>
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

        /// <summary>
        /// Handles authorization for deleting demos.
        /// Allows senior admins, head admins for the game type, or the demo owner.
        /// </summary>
        /// <param name="context">The authorization context.</param>
        /// <param name="requirement">The delete demo requirement.</param>
        private void HandleDeleteDemo(AuthorizationHandlerContext context, IAuthorizationRequirement requirement)
        {
            BaseAuthorizationHelper.CheckSeniorAdminAccess(context, requirement);

            if (context.Resource is Tuple<GameType, Guid> resource)
            {
                var (gameType, userProfileId) = resource;

                BaseAuthorizationHelper.CheckHeadAdminAccess(context, requirement, gameType);

                // Users can delete their own demos
                if (BaseAuthorizationHelper.IsResourceOwner(context, userProfileId))
                {
                    context.Succeed(requirement);
                }
            }
        }

        /// <summary>
        /// Handles authorization for accessing demos.
        /// Allows authenticated users or requests with demo manager API key.
        /// </summary>
        /// <param name="context">The authorization context.</param>
        /// <param name="requirement">The access demos requirement.</param>
        private void HandleAccessDemos(AuthorizationHandlerContext context, IAuthorizationRequirement requirement)
        {
            // Check if user is authenticated
            BaseAuthorizationHelper.CheckAuthenticated(context, requirement);

            // Check for demo manager API key in headers
            var httpContext = httpContextAccessor.HttpContext;
            if (httpContext?.Request.Headers.ContainsKey("demo-manager-auth-key") == true)
            {
                context.Succeed(requirement);
            }
        }
    }
}

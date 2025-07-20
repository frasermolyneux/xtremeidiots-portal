using Microsoft.AspNetCore.Authorization;

using XtremeIdiots.Portal.Web.Auth.Requirements;

namespace XtremeIdiots.Portal.Web.Auth.Handlers
{

    public class CredentialsAuthHandler : IAuthorizationHandler
    {

        public Task HandleAsync(AuthorizationHandlerContext context)
        {
            var pendingRequirements = context.PendingRequirements.ToList();

            foreach (var requirement in pendingRequirements)
            {
                switch (requirement)
                {
                    case AccessCredentials accessReq:
                        HandleAccessCredentials(context, accessReq);
                        break;
                }
            }

            return Task.CompletedTask;
        }

        #region Authorization Handlers

        private static void HandleAccessCredentials(AuthorizationHandlerContext context, IAuthorizationRequirement requirement)
        {
            BaseAuthorizationHelper.CheckClaimTypes(context, requirement, BaseAuthorizationHelper.ClaimGroups.CredentialsAccessLevels);
        }

        #endregion
    }
}
using Microsoft.AspNetCore.Authorization;
using XtremeIdiots.Portal.Repository.Abstractions.Constants.V1;
using XtremeIdiots.Portal.Web.Auth.Requirements;

namespace XtremeIdiots.Portal.Web.Auth.Handlers
{

    public class UsersAuthHandler : IAuthorizationHandler
    {

        public Task HandleAsync(AuthorizationHandlerContext context)
        {
            var pendingRequirements = context.PendingRequirements.ToList();

            foreach (var requirement in pendingRequirements)
            {
                switch (requirement)
                {
                    case AccessUsers:
                        HandleAccessUsers(context, requirement);
                        break;
                    case CreateUserClaim:
                        HandleCreateUserClaim(context, requirement);
                        break;
                    case DeleteUserClaim:
                        HandleDeleteUserClaim(context, requirement);
                        break;
                }
            }

            return Task.CompletedTask;
        }

        #region Authorization Handlers

        private static void HandleAccessUsers(AuthorizationHandlerContext context, IAuthorizationRequirement requirement)
        {
            BaseAuthorizationHelper.CheckClaimTypes(context, requirement, BaseAuthorizationHelper.ClaimGroups.SeniorAndHeadAdminOnly);
        }

        private static void HandleCreateUserClaim(AuthorizationHandlerContext context, IAuthorizationRequirement requirement)
        {
            BaseAuthorizationHelper.CheckSeniorAdminAccess(context, requirement);

            if (context.Resource is GameType gameType)
            {
                BaseAuthorizationHelper.CheckHeadAdminAccess(context, requirement, gameType);
            }
        }

        private static void HandleDeleteUserClaim(AuthorizationHandlerContext context, IAuthorizationRequirement requirement)
        {
            BaseAuthorizationHelper.CheckSeniorAdminAccess(context, requirement);

            if (context.Resource is GameType gameType)
            {
                BaseAuthorizationHelper.CheckHeadAdminAccess(context, requirement, gameType);
            }
        }

        #endregion
    }
}
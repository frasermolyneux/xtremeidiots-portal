using Microsoft.AspNetCore.Authorization;
using XtremeIdiots.Portal.Repository.Abstractions.Constants.V1;
using XtremeIdiots.Portal.Web.Auth.Requirements;

namespace XtremeIdiots.Portal.Web.Auth.Handlers;

/// <summary>
/// Handles authorization for user management operations in the portal
/// </summary>
public class UsersAuthHandler : IAuthorizationHandler
{
    /// <summary>
    /// Handles authorization requirements for user management operations
    /// </summary>
    /// <param name="context">The authorization handler context</param>
    /// <returns>A task representing the asynchronous operation</returns>
    public Task HandleAsync(AuthorizationHandlerContext context)
    {
        var pendingRequirements = context.PendingRequirements.ToList();

        foreach (var requirement in pendingRequirements)
        {
            switch (requirement)
            {
                case AccessUsers accessUsers:
                    HandleAccessUsers(context, accessUsers);
                    break;
                case CreateUserClaim createUserClaim:
                    HandleCreateUserClaim(context, createUserClaim);
                    break;
                case DeleteUserClaim deleteUserClaim:
                    HandleDeleteUserClaim(context, deleteUserClaim);
                    break;
                case PerformUserSearch performUserSearch:
                    HandlePerformUserSearch(context, performUserSearch);
                    break;
                default:
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

    private static void HandlePerformUserSearch(AuthorizationHandlerContext context, IAuthorizationRequirement requirement)
    {
        // Any admin level (including moderators) can perform user search for autocomplete scenarios
        BaseAuthorizationHelper.CheckClaimTypes(context, requirement, BaseAuthorizationHelper.ClaimGroups.AllAdminLevels);
    }

    #endregion
}
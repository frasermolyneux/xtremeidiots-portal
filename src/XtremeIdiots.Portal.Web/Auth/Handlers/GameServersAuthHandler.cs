using Microsoft.AspNetCore.Authorization;
using XtremeIdiots.Portal.Repository.Abstractions.Constants.V1;
using XtremeIdiots.Portal.Web.Auth.Requirements;

namespace XtremeIdiots.Portal.Web.Auth.Handlers;

/// <summary>
/// Authorization handler for game server operations
/// </summary>
public class GameServersAuthHandler : IAuthorizationHandler
{
    /// <summary>
    /// Handles authorization requirements for game server operations
    /// </summary>
    /// <param name="context">The authorization context containing user claims and requirements</param>
    /// <returns>A completed task indicating the authorization check is complete</returns>
    public Task HandleAsync(AuthorizationHandlerContext context)
    {
        var pendingRequirements = context.PendingRequirements.ToList();

        foreach (var requirement in pendingRequirements)
        {
            switch (requirement)
            {
                case AccessGameServers:
                    HandleAccessGameServers(context, requirement);
                    break;
                case CreateGameServer:
                    HandleCreateGameServer(context, requirement);
                    break;
                case DeleteGameServer:
                    HandleDeleteGameServer(context, requirement);
                    break;
                case EditGameServerFtp:
                    HandleEditGameServerFtp(context, requirement);
                    break;
                case EditGameServer:
                    HandleEditGameServer(context, requirement);
                    break;
                case EditGameServerRcon:
                    HandleEditGameServerRcon(context, requirement);
                    break;
                case ViewFtpCredential:
                    HandleViewFtpCredential(context, requirement);
                    break;
                case ViewGameServer:
                    HandleViewGameServer(context, requirement);
                    break;
                case ViewRconCredential:
                    HandleViewRconCredential(context, requirement);
                    break;
                default:
                    break;
            }
        }

        return Task.CompletedTask;
    }

    private static void HandleAccessGameServers(AuthorizationHandlerContext context, IAuthorizationRequirement requirement)
    {
        BaseAuthorizationHelper.CheckClaimTypes(context, requirement, BaseAuthorizationHelper.ClaimGroups.GameServerAccessLevels);
    }

    private static void HandleCreateGameServer(AuthorizationHandlerContext context, IAuthorizationRequirement requirement)
    {
        BaseAuthorizationHelper.CheckSeniorOrHeadAdminAccessWithResource(context, requirement);
    }

    private static void HandleDeleteGameServer(AuthorizationHandlerContext context, IAuthorizationRequirement requirement)
    {
        BaseAuthorizationHelper.CheckSeniorAdminAccess(context, requirement);
    }

    private static void HandleEditGameServerFtp(AuthorizationHandlerContext context, IAuthorizationRequirement requirement)
    {
        BaseAuthorizationHelper.CheckSeniorOrHeadAdminAccessWithResource(context, requirement);
    }

    private static void HandleEditGameServer(AuthorizationHandlerContext context, IAuthorizationRequirement requirement)
    {
        BaseAuthorizationHelper.CheckSeniorAdminAccess(context, requirement);

        if (context.Resource is GameType gameType)
        {
            BaseAuthorizationHelper.CheckCombinedGameServerAccess(context, requirement, gameType);
        }
    }

    private static void HandleEditGameServerRcon(AuthorizationHandlerContext context, IAuthorizationRequirement requirement)
    {
        BaseAuthorizationHelper.CheckSeniorOrHeadAdminAccessWithResource(context, requirement);
    }

    private static void HandleViewFtpCredential(AuthorizationHandlerContext context, IAuthorizationRequirement requirement)
    {
        BaseAuthorizationHelper.CheckSeniorAdminAccess(context, requirement);

        if (context.Resource is Tuple<GameType, Guid> resource)
        {
            var (gameType, gameServerId) = resource;
            BaseAuthorizationHelper.CheckHeadAdminAccess(context, requirement, gameType);
            BaseAuthorizationHelper.CheckFtpCredentialsAccess(context, requirement, gameServerId);
        }
    }

    private static void HandleViewGameServer(AuthorizationHandlerContext context, IAuthorizationRequirement requirement)
    {
        BaseAuthorizationHelper.CheckSeniorAdminAccess(context, requirement);

        if (context.Resource is GameType gameType)
        {
            BaseAuthorizationHelper.CheckCombinedGameServerAccess(context, requirement, gameType);
        }
    }

    private static void HandleViewRconCredential(AuthorizationHandlerContext context, IAuthorizationRequirement requirement)
    {
        BaseAuthorizationHelper.CheckSeniorOrLiveRconAccessWithResource(context, requirement);
    }
}
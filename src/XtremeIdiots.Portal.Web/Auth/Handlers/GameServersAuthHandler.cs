using Microsoft.AspNetCore.Authorization;
using XtremeIdiots.Portal.Repository.Abstractions.Constants.V1;
using XtremeIdiots.Portal.Web.Auth.Requirements;

namespace XtremeIdiots.Portal.Web.Auth.Handlers;

public class GameServersAuthHandler : IAuthorizationHandler
{

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

    #region Authorization Handlers

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

    #endregion
}
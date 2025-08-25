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

        if (context.Resource is Tuple<GameType, Guid> refTuple)
        {
            var gameType = refTuple.Item1;
            var gameServerId = refTuple.Item2;
            // Head admins should be able to view all FTP credentials for their game type without needing
            // individual per-server FtpCredentials claims. Previously they also needed a server scoped claim
            // which caused missing credentials on the credentials page.
            BaseAuthorizationHelper.CheckHeadAdminAccess(context, requirement, gameType);
            if (!context.HasSucceeded)
            {
                BaseAuthorizationHelper.CheckFtpCredentialsAccess(context, requirement, gameServerId);
            }
        }
        else if (context.Resource is (GameType gameType, Guid gameServerId))
        {
            BaseAuthorizationHelper.CheckHeadAdminAccess(context, requirement, gameType);
            if (!context.HasSucceeded)
            {
                BaseAuthorizationHelper.CheckFtpCredentialsAccess(context, requirement, gameServerId);
            }
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
        // Support both GameType and (GameType, GameServerId) resources.
        // SeniorAdmin short-circuit
        BaseAuthorizationHelper.CheckSeniorAdminAccess(context, requirement);

        if (context.Resource is Tuple<GameType, Guid> tupleResource)
        {
            var gameType = tupleResource.Item1;
            var gameServerId = tupleResource.Item2;
            // Allow head admin game-level access without needing per-server RCON credential claim
            BaseAuthorizationHelper.CheckHeadAdminAccess(context, requirement, gameType);
            if (!context.HasSucceeded)
            {
                BaseAuthorizationHelper.CheckGameAdminAccess(context, requirement, gameType);
                BaseAuthorizationHelper.CheckLiveRconAccess(context, requirement, gameType);
                BaseAuthorizationHelper.CheckRconCredentialsAccess(context, requirement, gameServerId);
            }
        }
        else if (context.Resource is (GameType gameType, Guid gameServerId))
        {
            BaseAuthorizationHelper.CheckHeadAdminAccess(context, requirement, gameType);
            if (!context.HasSucceeded)
            {
                BaseAuthorizationHelper.CheckGameAdminAccess(context, requirement, gameType);
                BaseAuthorizationHelper.CheckLiveRconAccess(context, requirement, gameType);
                BaseAuthorizationHelper.CheckRconCredentialsAccess(context, requirement, gameServerId);
            }
        }
        else if (context.Resource is GameType singleGameType)
        {
            // Original behaviour path when only a game type is supplied
            BaseAuthorizationHelper.CheckHeadAdminAccess(context, requirement, singleGameType);
            if (!context.HasSucceeded)
            {
                BaseAuthorizationHelper.CheckGameAdminAccess(context, requirement, singleGameType);
                BaseAuthorizationHelper.CheckLiveRconAccess(context, requirement, singleGameType);
            }
        }
    }
}
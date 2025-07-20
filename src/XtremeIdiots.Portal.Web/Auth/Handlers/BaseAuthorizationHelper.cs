using Microsoft.AspNetCore.Authorization;
using XtremeIdiots.Portal.Repository.Abstractions.Constants.V1;

namespace XtremeIdiots.Portal.Web.Auth.Handlers;

/// <summary>
/// Provides common authorization helper methods for policy-based authorization throughout the XtremeIdiots Portal
/// </summary>
public static class BaseAuthorizationHelper
{
    #region Constants

    /// <summary>
    /// Predefined claim groups for different authorization levels and access patterns
    /// </summary>
    public static class ClaimGroups
    {
        public readonly static string[] SeniorAdminOnly = [UserProfileClaimType.SeniorAdmin];

        public readonly static string[] AllAdminLevels =
        [
            UserProfileClaimType.SeniorAdmin,
            UserProfileClaimType.HeadAdmin,
            UserProfileClaimType.GameAdmin,
            UserProfileClaimType.Moderator
        ];

        public readonly static string[] BanFileMonitorLevels =
        [
            UserProfileClaimType.SeniorAdmin,
            UserProfileClaimType.HeadAdmin,
            UserProfileClaimType.BanFileMonitor
        ];

        public readonly static string[] CredentialsAccessLevels =
        [
            UserProfileClaimType.SeniorAdmin,
            UserProfileClaimType.HeadAdmin,
            UserProfileClaimType.GameAdmin,
            UserProfileClaimType.RconCredentials,
            UserProfileClaimType.FtpCredentials
        ];

        public readonly static string[] GameServerAccessLevels =
        [
            UserProfileClaimType.SeniorAdmin,
            UserProfileClaimType.HeadAdmin,
            UserProfileClaimType.GameServer
        ];

        public readonly static string[] AdminLevelsExcludingModerators =
        [
            UserProfileClaimType.SeniorAdmin,
            UserProfileClaimType.HeadAdmin,
            UserProfileClaimType.GameAdmin
        ];

        public readonly static string[] ServerAdminAccessLevels =
        [
            UserProfileClaimType.SeniorAdmin,
            UserProfileClaimType.HeadAdmin,
            UserProfileClaimType.GameAdmin,
            UserProfileClaimType.Moderator,
            UserProfileClaimType.ServerAdmin
        ];

        public readonly static string[] LiveRconAccessLevels =
        [
            UserProfileClaimType.SeniorAdmin,
            UserProfileClaimType.HeadAdmin,
            UserProfileClaimType.GameAdmin,
            UserProfileClaimType.LiveRcon
        ];

        public readonly static string[] SeniorAndHeadAdminOnly =
        [
            UserProfileClaimType.SeniorAdmin,
            UserProfileClaimType.HeadAdmin
        ];

        public readonly static string[] StatusAccessLevels =
        [
            UserProfileClaimType.SeniorAdmin,
            UserProfileClaimType.HeadAdmin,
            UserProfileClaimType.GameAdmin,
            UserProfileClaimType.BanFileMonitor
        ];
    }

    #endregion

    #region Core Authorization Checks

    /// <summary>
    /// Checks if the user has senior admin privileges (highest level access)
    /// </summary>
    /// <param name="context">The authorization context</param>
    /// <param name="requirement">The authorization requirement to succeed if access is granted</param>
    public static void CheckSeniorAdminAccess(AuthorizationHandlerContext context, IAuthorizationRequirement requirement)
    {
        if (context.User.Claims.Any(claim => claim.Type == UserProfileClaimType.SeniorAdmin))
            context.Succeed(requirement);
    }

    /// <summary>
    /// Checks if the user has any of the specified claim types
    /// </summary>
    /// <param name="context">The authorization context</param>
    /// <param name="requirement">The authorization requirement to succeed if access is granted</param>
    /// <param name="claimTypes">Array of claim types to check for</param>
    public static void CheckClaimTypes(AuthorizationHandlerContext context, IAuthorizationRequirement requirement, string[] claimTypes)
    {
        if (context.User.Claims.Any(claim => claimTypes.Contains(claim.Type)))
            context.Succeed(requirement);
    }

    /// <summary>
    /// Checks if the user has head admin access for the specified game type
    /// </summary>
    /// <param name="context">The authorization context</param>
    /// <param name="requirement">The authorization requirement to succeed if access is granted</param>
    /// <param name="gameType">The game type to check permissions for</param>
    public static void CheckHeadAdminAccess(AuthorizationHandlerContext context, IAuthorizationRequirement requirement, GameType gameType)
    {
        if (context.User.HasClaim(UserProfileClaimType.HeadAdmin, gameType.ToString()))
            context.Succeed(requirement);
    }

    /// <summary>
    /// Checks if the user has game admin access for the specified game type (includes head admin level)
    /// </summary>
    /// <param name="context">The authorization context</param>
    /// <param name="requirement">The authorization requirement to succeed if access is granted</param>
    /// <param name="gameType">The game type to check permissions for</param>
    public static void CheckGameAdminAccess(AuthorizationHandlerContext context, IAuthorizationRequirement requirement, GameType gameType)
    {
        var gameTypeString = gameType.ToString();

        if (context.User.HasClaim(UserProfileClaimType.HeadAdmin, gameTypeString) ||
            context.User.HasClaim(UserProfileClaimType.GameAdmin, gameTypeString))
        {
            context.Succeed(requirement);
        }
    }

    /// <summary>
    /// Checks if the user has moderator access for the specified game type (includes game admin level)
    /// </summary>
    /// <param name="context">The authorization context</param>
    /// <param name="requirement">The authorization requirement to succeed if access is granted</param>
    /// <param name="gameType">The game type to check permissions for</param>
    public static void CheckModeratorAccess(AuthorizationHandlerContext context, IAuthorizationRequirement requirement, GameType gameType)
    {
        var gameTypeString = gameType.ToString();

        if (context.User.HasClaim(UserProfileClaimType.Moderator, gameTypeString) ||
            context.User.HasClaim(UserProfileClaimType.GameAdmin, gameTypeString))
        {
            context.Succeed(requirement);
        }
    }

    #endregion

    #region Composite Authorization Checks

    /// <summary>
    /// Checks senior admin access first, then game admin access based on resource context
    /// </summary>
    /// <param name="context">The authorization context</param>
    /// <param name="requirement">The authorization requirement to succeed if access is granted</param>
    public static void CheckSeniorOrGameAdminAccessWithResource(AuthorizationHandlerContext context, IAuthorizationRequirement requirement)
    {
        if (context.User.Claims.Any(claim => claim.Type == UserProfileClaimType.SeniorAdmin))
        {
            context.Succeed(requirement);
            return;
        }

        if (context.Resource is GameType gameType)
            CheckGameAdminAccess(context, requirement, gameType);
    }

    /// <summary>
    /// Checks senior admin access first, then head admin access based on resource context
    /// </summary>
    /// <param name="context">The authorization context</param>
    /// <param name="requirement">The authorization requirement to succeed if access is granted</param>
    public static void CheckSeniorOrHeadAdminAccessWithResource(AuthorizationHandlerContext context, IAuthorizationRequirement requirement)
    {
        if (context.User.Claims.Any(claim => claim.Type == UserProfileClaimType.SeniorAdmin))
        {
            context.Succeed(requirement);
            return;
        }

        if (context.Resource is GameType gameType)
            CheckHeadAdminAccess(context, requirement, gameType);
    }

    /// <summary>
    /// Checks senior admin access first, then game type and server specific access
    /// </summary>
    /// <param name="context">The authorization context</param>
    /// <param name="requirement">The authorization requirement to succeed if access is granted</param>
    public static void CheckSeniorOrGameTypeServerAccessWithResource(AuthorizationHandlerContext context, IAuthorizationRequirement requirement)
    {
        if (context.User.Claims.Any(claim => claim.Type == UserProfileClaimType.SeniorAdmin))
        {
            context.Succeed(requirement);
            return;
        }

        if (context.Resource is Tuple<GameType, Guid> resource)
        {
            var (gameType, gameServerId) = resource;
            CheckGameTypeAndServerAccess(context, requirement, gameType, gameServerId);
        }
    }

    /// <summary>
    /// Checks senior admin access first, then multiple levels of game access
    /// </summary>
    /// <param name="context">The authorization context</param>
    /// <param name="requirement">The authorization requirement to succeed if access is granted</param>
    public static void CheckSeniorOrMultipleGameAccessWithResource(AuthorizationHandlerContext context, IAuthorizationRequirement requirement)
    {
        if (context.User.Claims.Any(claim => claim.Type == UserProfileClaimType.SeniorAdmin))
        {
            context.Succeed(requirement);
            return;
        }

        if (context.Resource is GameType gameType)
        {
            CheckHeadAdminAccess(context, requirement, gameType);
            CheckGameAdminAccess(context, requirement, gameType);
            CheckModeratorAccess(context, requirement, gameType);
        }
    }

    /// <summary>
    /// Checks senior admin access first, then live RCON access for the game type
    /// </summary>
    /// <param name="context">The authorization context</param>
    /// <param name="requirement">The authorization requirement to succeed if access is granted</param>
    public static void CheckSeniorOrLiveRconAccessWithResource(AuthorizationHandlerContext context, IAuthorizationRequirement requirement)
    {
        if (context.User.Claims.Any(claim => claim.Type == UserProfileClaimType.SeniorAdmin))
        {
            context.Succeed(requirement);
            return;
        }

        if (context.Resource is GameType gameType)
        {
            CheckGameAdminAccess(context, requirement, gameType);
            CheckLiveRconAccess(context, requirement, gameType);
        }
    }

    #endregion

    #region Server-Specific Authorization

    /// <summary>
    /// Checks if the user has ban file monitor access for a specific game server
    /// </summary>
    /// <param name="context">The authorization context</param>
    /// <param name="requirement">The authorization requirement to succeed if access is granted</param>
    /// <param name="gameServerId">The game server ID to check permissions for</param>
    public static void CheckBanFileMonitorAccess(AuthorizationHandlerContext context, IAuthorizationRequirement requirement, Guid gameServerId)
    {
        if (context.User.HasClaim(UserProfileClaimType.BanFileMonitor, gameServerId.ToString()))
            context.Succeed(requirement);
    }

    /// <summary>
    /// Checks both game type and server-specific access permissions
    /// </summary>
    /// <param name="context">The authorization context</param>
    /// <param name="requirement">The authorization requirement to succeed if access is granted</param>
    /// <param name="gameType">The game type to check permissions for</param>
    /// <param name="gameServerId">The game server ID to check permissions for</param>
    public static void CheckGameTypeAndServerAccess(AuthorizationHandlerContext context, IAuthorizationRequirement requirement, GameType gameType, Guid gameServerId)
    {
        CheckHeadAdminAccess(context, requirement, gameType);
        CheckBanFileMonitorAccess(context, requirement, gameServerId);
    }

    #endregion

    #region Game Server Authorization

    /// <summary>
    /// Checks if the user has game server access for the specified game type
    /// </summary>
    /// <param name="context">The authorization context</param>
    /// <param name="requirement">The authorization requirement to succeed if access is granted</param>
    /// <param name="gameType">The game type to check permissions for</param>
    public static void CheckGameServerAccess(AuthorizationHandlerContext context, IAuthorizationRequirement requirement, GameType gameType)
    {
        if (context.User.HasClaim(UserProfileClaimType.GameServer, gameType.ToString()))
            context.Succeed(requirement);
    }

    /// <summary>
    /// Checks if the user has RCON credentials access for a specific game server
    /// </summary>
    /// <param name="context">The authorization context</param>
    /// <param name="requirement">The authorization requirement to succeed if access is granted</param>
    /// <param name="gameServerId">The game server ID to check permissions for</param>
    public static void CheckRconCredentialsAccess(AuthorizationHandlerContext context, IAuthorizationRequirement requirement, Guid gameServerId)
    {
        if (context.User.HasClaim(UserProfileClaimType.RconCredentials, gameServerId.ToString()))
            context.Succeed(requirement);
    }

    /// <summary>
    /// Checks if the user has FTP credentials access for a specific game server
    /// </summary>
    /// <param name="context">The authorization context</param>
    /// <param name="requirement">The authorization requirement to succeed if access is granted</param>
    /// <param name="gameServerId">The game server ID to check permissions for</param>
    public static void CheckFtpCredentialsAccess(AuthorizationHandlerContext context, IAuthorizationRequirement requirement, Guid gameServerId)
    {
        if (context.User.HasClaim(UserProfileClaimType.FtpCredentials, gameServerId.ToString()))
            context.Succeed(requirement);
    }

    /// <summary>
    /// Combines head admin and game server access checks for comprehensive server access
    /// </summary>
    /// <param name="context">The authorization context</param>
    /// <param name="requirement">The authorization requirement to succeed if access is granted</param>
    /// <param name="gameType">The game type to check permissions for</param>
    public static void CheckCombinedGameServerAccess(AuthorizationHandlerContext context, IAuthorizationRequirement requirement, GameType gameType)
    {
        CheckHeadAdminAccess(context, requirement, gameType);
        CheckGameServerAccess(context, requirement, gameType);
    }

    /// <summary>
    /// Checks if the user has live RCON access for the specified game type
    /// </summary>
    /// <param name="context">The authorization context</param>
    /// <param name="requirement">The authorization requirement to succeed if access is granted</param>
    /// <param name="gameType">The game type to check permissions for</param>
    public static void CheckLiveRconAccess(AuthorizationHandlerContext context, IAuthorizationRequirement requirement, GameType gameType)
    {
        if (context.User.HasClaim(UserProfileClaimType.LiveRcon, gameType.ToString()))
            context.Succeed(requirement);
    }

    #endregion

    #region Utility Methods

    /// <summary>
    /// Retrieves the XtremeIdiots ID from the user's claims
    /// </summary>
    /// <param name="context">The authorization context</param>
    /// <returns>The XtremeIdiots ID if found, otherwise null</returns>
    public static string? GetUserXtremeIdiotsId(AuthorizationHandlerContext context)
    {
        return context.User.FindFirst(UserProfileClaimType.XtremeIdiotsId)?.Value;
    }

    /// <summary>
    /// Determines if the current user is the owner of the specified action
    /// </summary>
    /// <param name="context">The authorization context</param>
    /// <param name="adminId">The admin ID to compare against the user's ID</param>
    /// <returns>True if the user is the action owner, false otherwise</returns>
    public static bool IsActionOwner(AuthorizationHandlerContext context, string? adminId)
    {
        var userXtremeId = GetUserXtremeIdiotsId(context);
        return userXtremeId == adminId;
    }

    /// <summary>
    /// Retrieves the user profile ID from the user's claims
    /// </summary>
    /// <param name="context">The authorization context</param>
    /// <returns>The user profile ID if found, otherwise null</returns>
    public static string? GetUserProfileId(AuthorizationHandlerContext context)
    {
        return context.User.FindFirst(UserProfileClaimType.UserProfileId)?.Value;
    }

    /// <summary>
    /// Determines if the current user is the owner of the specified resource
    /// </summary>
    /// <param name="context">The authorization context</param>
    /// <param name="resourceUserProfileId">The resource owner's user profile ID</param>
    /// <returns>True if the user is the resource owner, false otherwise</returns>
    public static bool IsResourceOwner(AuthorizationHandlerContext context, Guid resourceUserProfileId)
    {
        var userProfileId = GetUserProfileId(context);
        return userProfileId is not null && userProfileId == resourceUserProfileId.ToString();
    }

    /// <summary>
    /// Checks if the user is authenticated
    /// </summary>
    /// <param name="context">The authorization context</param>
    /// <param name="requirement">The authorization requirement to succeed if access is granted</param>
    public static void CheckAuthenticated(AuthorizationHandlerContext context, IAuthorizationRequirement requirement)
    {
        if (context.User.Identity?.IsAuthenticated == true)
            context.Succeed(requirement);
    }

    #endregion
}
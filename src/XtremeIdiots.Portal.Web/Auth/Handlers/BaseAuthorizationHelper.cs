using Microsoft.AspNetCore.Authorization;
using XtremeIdiots.Portal.Repository.Abstractions.Constants.V1;

namespace XtremeIdiots.Portal.Web.Auth.Handlers
{
    /// <summary>
    /// Provides common authorization helper methods for use across authorization handlers.
    /// Contains reusable claim checking and permission validation logic.
    /// </summary>
    public static class BaseAuthorizationHelper
    {
        #region Constants

        /// <summary>
        /// Predefined claim groups for common authorization scenarios.
        /// </summary>
        public static class ClaimGroups
        {
            public static readonly string[] SeniorAdminOnly = { UserProfileClaimType.SeniorAdmin };

            public static readonly string[] AllAdminLevels =
            {
                UserProfileClaimType.SeniorAdmin,
                UserProfileClaimType.HeadAdmin,
                UserProfileClaimType.GameAdmin,
                UserProfileClaimType.Moderator
            };

            public static readonly string[] BanFileMonitorLevels =
            {
                UserProfileClaimType.SeniorAdmin,
                UserProfileClaimType.HeadAdmin,
                UserProfileClaimType.BanFileMonitor
            };

            public static readonly string[] CredentialsAccessLevels =
            {
                UserProfileClaimType.SeniorAdmin,
                UserProfileClaimType.HeadAdmin,
                UserProfileClaimType.GameAdmin,
                UserProfileClaimType.RconCredentials,
                UserProfileClaimType.FtpCredentials
            };

            public static readonly string[] GameServerAccessLevels =
            {
                UserProfileClaimType.SeniorAdmin,
                UserProfileClaimType.HeadAdmin,
                UserProfileClaimType.GameServer
            };

            public static readonly string[] AdminLevelsExcludingModerators =
            {
                UserProfileClaimType.SeniorAdmin,
                UserProfileClaimType.HeadAdmin,
                UserProfileClaimType.GameAdmin
            };

            public static readonly string[] ServerAdminAccessLevels =
            {
                UserProfileClaimType.SeniorAdmin,
                UserProfileClaimType.HeadAdmin,
                UserProfileClaimType.GameAdmin,
                UserProfileClaimType.Moderator,
                UserProfileClaimType.ServerAdmin
            };

            public static readonly string[] LiveRconAccessLevels =
            {
                UserProfileClaimType.SeniorAdmin,
                UserProfileClaimType.HeadAdmin,
                UserProfileClaimType.GameAdmin,
                UserProfileClaimType.LiveRcon
            };

            public static readonly string[] SeniorAndHeadAdminOnly =
            {
                UserProfileClaimType.SeniorAdmin,
                UserProfileClaimType.HeadAdmin
            };

            public static readonly string[] StatusAccessLevels =
            {
                UserProfileClaimType.SeniorAdmin,
                UserProfileClaimType.HeadAdmin,
                UserProfileClaimType.GameAdmin,
                UserProfileClaimType.BanFileMonitor
            };
        }

        #endregion

        #region Core Authorization Checks

        /// <summary>
        /// Checks if the user has senior admin access and succeeds the requirement if they do.
        /// </summary>
        /// <param name="context">The authorization context.</param>
        /// <param name="requirement">The requirement to potentially succeed.</param>
        public static void CheckSeniorAdminAccess(AuthorizationHandlerContext context, IAuthorizationRequirement requirement)
        {
            if (context.User.Claims.Any(claim => claim.Type == UserProfileClaimType.SeniorAdmin))
            {
                context.Succeed(requirement);
            }
        }

        /// <summary>
        /// Checks if the user has any of the specified claim types.
        /// </summary>
        /// <param name="context">The authorization context.</param>
        /// <param name="requirement">The requirement to potentially succeed.</param>
        /// <param name="claimTypes">Array of claim types to check for.</param>
        public static void CheckClaimTypes(AuthorizationHandlerContext context, IAuthorizationRequirement requirement, string[] claimTypes)
        {
            if (context.User.Claims.Any(claim => claimTypes.Contains(claim.Type)))
            {
                context.Succeed(requirement);
            }
        }

        /// <summary>
        /// Checks if the user has head admin access for the given game type.
        /// </summary>
        /// <param name="context">The authorization context.</param>
        /// <param name="requirement">The requirement to potentially succeed.</param>
        /// <param name="gameType">The game type to check permissions for.</param>
        public static void CheckHeadAdminAccess(AuthorizationHandlerContext context, IAuthorizationRequirement requirement, GameType gameType)
        {
            if (context.User.HasClaim(UserProfileClaimType.HeadAdmin, gameType.ToString()))
            {
                context.Succeed(requirement);
            }
        }

        /// <summary>
        /// Checks if the user has game-specific admin access for the given game type.
        /// </summary>
        /// <param name="context">The authorization context.</param>
        /// <param name="requirement">The requirement to potentially succeed.</param>
        /// <param name="gameType">The game type to check permissions for.</param>
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
        /// Checks if the user has moderator-level access for the given game type.
        /// </summary>
        /// <param name="context">The authorization context.</param>
        /// <param name="requirement">The requirement to potentially succeed.</param>
        /// <param name="gameType">The game type to check permissions for.</param>
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
        /// Checks senior admin access OR game admin access for the game type resource.
        /// Used by: MapsAuthHandler (8 methods), AdminActionsAuthHandler (3 methods), etc.
        /// </summary>
        /// <param name="context">The authorization context.</param>
        /// <param name="requirement">The requirement to potentially succeed.</param>
        public static void CheckSeniorOrGameAdminAccessWithResource(AuthorizationHandlerContext context, IAuthorizationRequirement requirement)
        {
            // Check senior admin first (most privileged) - early return optimization
            if (context.User.Claims.Any(claim => claim.Type == UserProfileClaimType.SeniorAdmin))
            {
                context.Succeed(requirement);
                return;
            }

            // Only check game admin if senior admin failed
            if (context.Resource is GameType gameType)
            {
                CheckGameAdminAccess(context, requirement, gameType);
            }
        }

        /// <summary>
        /// Checks senior admin access OR head admin access for the game type resource.
        /// Used by: UsersAuthHandler (2 methods), ServerAdminAuthHandler (1 method), etc.
        /// </summary>
        /// <param name="context">The authorization context.</param>
        /// <param name="requirement">The requirement to potentially succeed.</param>
        public static void CheckSeniorOrHeadAdminAccessWithResource(AuthorizationHandlerContext context, IAuthorizationRequirement requirement)
        {
            // Check senior admin first (most privileged) - early return optimization
            if (context.User.Claims.Any(claim => claim.Type == UserProfileClaimType.SeniorAdmin))
            {
                context.Succeed(requirement);
                return;
            }

            // Only check head admin if senior admin failed
            if (context.Resource is GameType gameType)
            {
                CheckHeadAdminAccess(context, requirement, gameType);
            }
        }

        /// <summary>
        /// Checks senior admin access OR game type and server access for tuple resource.
        /// Used by: BanFileMonitorsAuthHandler (4 methods), AdminActionsAuthHandler (2 methods), etc.
        /// </summary>
        /// <param name="context">The authorization context.</param>
        /// <param name="requirement">The requirement to potentially succeed.</param>
        public static void CheckSeniorOrGameTypeServerAccessWithResource(AuthorizationHandlerContext context, IAuthorizationRequirement requirement)
        {
            // Check senior admin first (most privileged) - early return optimization
            if (context.User.Claims.Any(claim => claim.Type == UserProfileClaimType.SeniorAdmin))
            {
                context.Succeed(requirement);
                return;
            }

            // Only check game type and server access if senior admin failed
            if (context.Resource is Tuple<GameType, Guid> resource)
            {
                var (gameType, gameServerId) = resource;
                CheckGameTypeAndServerAccess(context, requirement, gameType, gameServerId);
            }
        }

        /// <summary>
        /// Checks senior admin access OR multiple game-specific permissions for the game type resource.
        /// Used by: ServerAdminAuthHandler for complex chat log permissions, etc.
        /// </summary>
        /// <param name="context">The authorization context.</param>
        /// <param name="requirement">The requirement to potentially succeed.</param>
        public static void CheckSeniorOrMultipleGameAccessWithResource(AuthorizationHandlerContext context, IAuthorizationRequirement requirement)
        {
            // Check senior admin first (most privileged) - early return optimization
            if (context.User.Claims.Any(claim => claim.Type == UserProfileClaimType.SeniorAdmin))
            {
                context.Succeed(requirement);
                return;
            }

            // Only check multiple game permissions if senior admin failed
            if (context.Resource is GameType gameType)
            {
                CheckHeadAdminAccess(context, requirement, gameType);
                CheckGameAdminAccess(context, requirement, gameType);
                CheckModeratorAccess(context, requirement, gameType);
            }
        }

        /// <summary>
        /// Checks senior admin access OR live RCON access with game admin permissions for the game type resource.
        /// Used by: ServerAdminAuthHandler for live RCON viewing, etc.
        /// </summary>
        /// <param name="context">The authorization context.</param>
        /// <param name="requirement">The requirement to potentially succeed.</param>
        public static void CheckSeniorOrLiveRconAccessWithResource(AuthorizationHandlerContext context, IAuthorizationRequirement requirement)
        {
            // Check senior admin first (most privileged) - early return optimization
            if (context.User.Claims.Any(claim => claim.Type == UserProfileClaimType.SeniorAdmin))
            {
                context.Succeed(requirement);
                return;
            }

            // Only check game admin and live RCON if senior admin failed
            if (context.Resource is GameType gameType)
            {
                CheckGameAdminAccess(context, requirement, gameType);
                CheckLiveRconAccess(context, requirement, gameType);
            }
        }

        #endregion

        #region Server-Specific Authorization

        /// <summary>
        /// Checks if the user has ban file monitor access for a specific game server.
        /// </summary>
        /// <param name="context">The authorization context.</param>
        /// <param name="requirement">The requirement to potentially succeed.</param>
        /// <param name="gameServerId">The game server ID to check permissions for.</param>
        public static void CheckBanFileMonitorAccess(AuthorizationHandlerContext context, IAuthorizationRequirement requirement, Guid gameServerId)
        {
            if (context.User.HasClaim(UserProfileClaimType.BanFileMonitor, gameServerId.ToString()))
            {
                context.Succeed(requirement);
            }
        }

        /// <summary>
        /// Checks combined game type and server-specific permissions for ban file monitoring.
        /// </summary>
        /// <param name="context">The authorization context.</param>
        /// <param name="requirement">The requirement to potentially succeed.</param>
        /// <param name="gameType">The game type to check permissions for.</param>
        /// <param name="gameServerId">The game server ID to check permissions for.</param>
        public static void CheckGameTypeAndServerAccess(AuthorizationHandlerContext context, IAuthorizationRequirement requirement, GameType gameType, Guid gameServerId)
        {
            CheckHeadAdminAccess(context, requirement, gameType);
            CheckBanFileMonitorAccess(context, requirement, gameServerId);
        }

        #endregion

        #region Game Server Authorization

        /// <summary>
        /// Checks if the user has game server access for the given game type.
        /// </summary>
        /// <param name="context">The authorization context.</param>
        /// <param name="requirement">The requirement to potentially succeed.</param>
        /// <param name="gameType">The game type to check permissions for.</param>
        public static void CheckGameServerAccess(AuthorizationHandlerContext context, IAuthorizationRequirement requirement, GameType gameType)
        {
            if (context.User.HasClaim(UserProfileClaimType.GameServer, gameType.ToString()))
            {
                context.Succeed(requirement);
            }
        }

        /// <summary>
        /// Checks if the user has RCON credentials access for a specific game server.
        /// </summary>
        /// <param name="context">The authorization context.</param>
        /// <param name="requirement">The requirement to potentially succeed.</param>
        /// <param name="gameServerId">The game server ID to check permissions for.</param>
        public static void CheckRconCredentialsAccess(AuthorizationHandlerContext context, IAuthorizationRequirement requirement, Guid gameServerId)
        {
            if (context.User.HasClaim(UserProfileClaimType.RconCredentials, gameServerId.ToString()))
            {
                context.Succeed(requirement);
            }
        }

        /// <summary>
        /// Checks if the user has FTP credentials access for a specific game server.
        /// </summary>
        /// <param name="context">The authorization context.</param>
        /// <param name="requirement">The requirement to potentially succeed.</param>
        /// <param name="gameServerId">The game server ID to check permissions for.</param>
        public static void CheckFtpCredentialsAccess(AuthorizationHandlerContext context, IAuthorizationRequirement requirement, Guid gameServerId)
        {
            if (context.User.HasClaim(UserProfileClaimType.FtpCredentials, gameServerId.ToString()))
            {
                context.Succeed(requirement);
            }
        }

        /// <summary>
        /// Checks combined game server access including head admin and game server permissions.
        /// </summary>
        /// <param name="context">The authorization context.</param>
        /// <param name="requirement">The requirement to potentially succeed.</param>
        /// <param name="gameType">The game type to check permissions for.</param>
        public static void CheckCombinedGameServerAccess(AuthorizationHandlerContext context, IAuthorizationRequirement requirement, GameType gameType)
        {
            CheckHeadAdminAccess(context, requirement, gameType);
            CheckGameServerAccess(context, requirement, gameType);
        }

        /// <summary>
        /// Checks if the user has live RCON access for the given game type.
        /// </summary>
        /// <param name="context">The authorization context.</param>
        /// <param name="requirement">The requirement to potentially succeed.</param>
        /// <param name="gameType">The game type to check permissions for.</param>
        public static void CheckLiveRconAccess(AuthorizationHandlerContext context, IAuthorizationRequirement requirement, GameType gameType)
        {
            if (context.User.HasClaim(UserProfileClaimType.LiveRcon, gameType.ToString()))
            {
                context.Succeed(requirement);
            }
        }

        #endregion

        #region Utility Methods

        /// <summary>
        /// Gets the XtremeIdiots ID for the current user.
        /// </summary>
        /// <param name="context">The authorization context.</param>
        /// <returns>The user's XtremeIdiots ID, or null if not found.</returns>
        public static string? GetUserXtremeIdiotsId(AuthorizationHandlerContext context)
        {
            return context.User.FindFirst(UserProfileClaimType.XtremeIdiotsId)?.Value;
        }

        /// <summary>
        /// Checks if the user is the owner of an action based on admin ID comparison.
        /// </summary>
        /// <param name="context">The authorization context.</param>
        /// <param name="adminId">The admin ID to check ownership against.</param>
        /// <returns>True if the user is the owner, false otherwise.</returns>
        public static bool IsActionOwner(AuthorizationHandlerContext context, string? adminId)
        {
            var userXtremeId = GetUserXtremeIdiotsId(context);
            return userXtremeId == adminId;
        }

        /// <summary>
        /// Gets the UserProfileId for the current user.
        /// </summary>
        /// <param name="context">The authorization context.</param>
        /// <returns>The user's UserProfileId, or null if not found.</returns>
        public static string? GetUserProfileId(AuthorizationHandlerContext context)
        {
            return context.User.FindFirst(UserProfileClaimType.UserProfileId)?.Value;
        }

        /// <summary>
        /// Checks if the user is the owner of a resource based on UserProfileId comparison.
        /// </summary>
        /// <param name="context">The authorization context.</param>
        /// <param name="resourceUserProfileId">The UserProfileId of the resource owner.</param>
        /// <returns>True if the user owns the resource, false otherwise.</returns>
        public static bool IsResourceOwner(AuthorizationHandlerContext context, Guid resourceUserProfileId)
        {
            var userProfileId = GetUserProfileId(context);
            return userProfileId != null && userProfileId == resourceUserProfileId.ToString();
        }

        /// <summary>
        /// Checks if the user is authenticated.
        /// </summary>
        /// <param name="context">The authorization context.</param>
        /// <param name="requirement">The requirement to potentially succeed.</param>
        public static void CheckAuthenticated(AuthorizationHandlerContext context, IAuthorizationRequirement requirement)
        {
            if (context.User.Identity?.IsAuthenticated == true)
            {
                context.Succeed(requirement);
            }
        }

        #endregion
    }
}
